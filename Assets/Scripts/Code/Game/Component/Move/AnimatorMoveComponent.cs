using UnityEngine;

namespace TaoTie
{

	public partial class AnimatorMoveComponent : Component, IComponent<ConfigAnimatorMove>, IFixedUpdate
	{
		public ConfigAnimatorMove Config { get; private set; }
		private Unit unit => parent as Unit;

		public MoveInput CharacterInput;

		//References to attached components;
		protected Transform transform;
		protected Mover mover;

		protected CeilingDetector ceilingDetector;

		//Jump key variables;
		bool jumpInputIsLocked = false;
		bool jumpKeyWasPressed = false;
		bool jumpKeyWasLetGo = false;
		bool jumpKeyIsPressed = false;

		//How fast the controller can change direction while in the air;
		//Higher values result in more air control;
		public float airControlRate = 2f;

		//Jump speed;
		public float jumpSpeed = 5f;

		//Jump duration variables;
		public float jumpDuration = 0.2f;
		float currentJumpStartTime = 0f;

		//'AirFriction' determines how fast the controller loses its momentum while in the air;
		//'GroundFriction' is used instead, if the controller is grounded;
		public float airFriction = 0.5f;
		public float groundFriction = 100f;

		//Current momentum;
		protected Vector3 momentum = Vector3.zero;

		//Saved velocity from last frame;
		Vector3 savedVelocity = Vector3.zero;

		//Saved horizontal movement velocity from last frame;
		Vector3 savedMovementVelocity = Vector3.zero;

		//Amount of downward gravity;
		public float gravity = 30f;

		[Tooltip("How fast the character will slide down steep slopes.")]
		public float slideGravity = 5f;

		//Acceptable slope angle limit;
		public float slopeLimit = 80f;

		[Tooltip("Whether to calculate and apply momentum relative to the controller's transform.")]
		public bool useLocalMomentum = false;

		//Enum describing basic controller states; 
		public enum ControllerState
		{
			Grounded,
			Sliding,
			Falling,
			Rising,
			Jumping
		}

		public ControllerState CurrentControllerState { get; private set; }= ControllerState.Falling;

		[Tooltip(
			"Optional camera transform used for calculating movement direction. If assigned, character movement will take camera view into account.")]
		public Transform cameraTransform;

		//Get references to all necessary components;
		public void Init(ConfigAnimatorMove config)
		{
			Config = config;
			CharacterInput = new MoveInput();
			lastAnimatorMoveTime = GameTimerManager.Instance.GetTimeNow();
			InitAsync().Coroutine();
		}
		private async ETTask InitAsync()
		{
			var model = parent.GetComponent<UnitModelComponent>();
			await model.WaitLoadGameObjectOver();
			if (model.IsDispose) return;
			mover = model.EntityView.GetComponent<Mover>();
			ceilingDetector = model.EntityView.GetComponent<CeilingDetector>();
			if (mover != null)
			{
				mover.enabled = true;
				mover.OnAnimatorMoveEvt = OnAnimatorMove;
			}
			transform = model.EntityView;
			cameraTransform = CameraManager.Instance.MainCamera().transform;
		}

		public void Destroy()
		{
			if (mover != null)
			{
				mover.OnAnimatorMoveEvt = null;
				mover.enabled = false;
				mover = null;
			}

			transform = null;
			cameraTransform = null;
			CharacterInput = null;

		}

		public void FixedUpdate()
		{
			if (transform == null || mover == null) return;
			ControllerUpdate();
			HandlerForward();
			HandleJumpKeyInput();
			unit.SyncViewPosition(transform.position);
		}

		void HandlerForward()
		{
			// if(!canTurn) return;
			if (!(parent is Avatar)) return;
			var lookDir = CalculateLookDirection();
			if (lookDir != Vector3.zero)
			{
				Vector3 dir;
				var angle = Vector3.Angle(lookDir, transform.forward);
				if (angle > 3)
				{
					var flag = Vector3.Cross(lookDir, transform.forward);
					var deltaTime = Time.fixedDeltaTime;
					if (this is IUpdate)
					{
						deltaTime = GameTimerManager.Instance.GetDeltaTime() / 1000f;
					}
					var temp = (flag.y > 0 ? -1 : 1) * CharacterInput.RolateSpeed * deltaTime;
					if (Mathf.Abs(temp - angle) < 0) temp = angle;
					dir = Quaternion.Euler(0, temp, 0) * transform.forward;
				}
				else
				{
					dir = lookDir;
				}

				unit.Rotation = Quaternion.LookRotation(dir, Vector3.up);
			}
		}

		protected Vector3 CalculateLookDirection()
		{
			//If no character input script is attached to this object, return;
			if (CharacterInput == null)
				return Vector3.zero;

			Vector3 v = Vector3.zero;

			//If a camera transform has been assigned, use the assigned transform's axes for movement direction;
			//Project movement direction so movement stays parallel to the ground;
			if (parent is Avatar)
			{
				v += Vector3.ProjectOnPlane(cameraTransform.right, transform.up).normalized *
				     CharacterInput.Direction.x;
				v += Vector3.ProjectOnPlane(cameraTransform.forward, transform.up).normalized *
				     CharacterInput.Direction.z;
			}
			else
			{
				v += transform.right * CharacterInput.Direction.x;
				v += transform.forward * CharacterInput.Direction.z;
			}

			v.Normalize();
			return v;
		}

		//Handle jump booleans for later use in FixedUpdate;
		void HandleJumpKeyInput()
		{
			bool _newJumpKeyPressedState = IsJumpKeyPressed();

			if (jumpKeyIsPressed == false && _newJumpKeyPressedState == true)
				jumpKeyWasPressed = true;

			if (jumpKeyIsPressed == true && _newJumpKeyPressedState == false)
			{
				jumpKeyWasLetGo = true;
				jumpInputIsLocked = false;
			}

			jumpKeyIsPressed = _newJumpKeyPressedState;
		}

		//Update controller;
		//This function must be called every fixed update, in order for the controller to work correctly;
		void ControllerUpdate()
		{
			//Check if mover is grounded;
			mover.CheckForGround();

			//Determine controller state;
			CurrentControllerState = DetermineControllerState();

			//Apply friction and gravity to 'momentum';
			HandleMomentum();

			//Check if the player has initiated a jump;
			HandleJumping();

			//Calculate movement velocity;
			Vector3 velocity = Vector3.zero;
			if (CurrentControllerState == ControllerState.Grounded)
				velocity = CalculateMovementVelocity();

			//If local momentum is used, transform momentum into world space first;
			Vector3 worldMomentum = momentum;
			if (useLocalMomentum)
				worldMomentum = transform.localToWorldMatrix * momentum;

			//Add current momentum to velocity;
			velocity += worldMomentum;

			//If player is grounded or sliding on a slope, extend mover's sensor range;
			//This enables the player to walk up/down stairs and slopes without losing ground contact;
			mover.SetExtendSensorRange(IsGrounded());

			//Set mover velocity;		
			mover.SetVelocity(velocity);

			//Store velocity for next frame;
			savedVelocity = velocity;

			//Save controller movement velocity;
			savedMovementVelocity = CalculateMovementVelocity();

			//Reset jump key booleans;
			jumpKeyWasLetGo = false;
			jumpKeyWasPressed = false;

			//Reset ceiling detector, if one is attached to this gameobject;
			if (ceilingDetector != null)
				ceilingDetector.ResetFlags();
		}

		//Calculate and return movement direction based on player input;
		//This function can be overridden by inheriting scripts to implement different player controls;
		protected Vector3 CalculateMovementVelocity()
		{
			//If no character input script is attached to this object, return;
			if (CharacterInput == null)
				return Vector3.zero;

			Vector3 _velocity = Vector3.zero;

			_velocity += transform.right * CharacterInput.GetHorizontalMovementInput();
			_velocity += transform.forward * CharacterInput.GetVerticalMovementInput();

			return _velocity;
		}

		//Returns 'true' if the player presses the jump key;
		protected bool IsJumpKeyPressed()
		{
			//If no character input script is attached to this object, return;
			if (CharacterInput == null)
				return false;

			return CharacterInput.IsJumpKeyPressed();
		}

		//Determine current controller state based on current momentum and whether the controller is grounded (or not);
		//Handle state transitions;
		ControllerState DetermineControllerState()
		{
			//Check if vertical momentum is pointing upwards;
			bool _isRising = IsRisingOrFalling() && (VectorMath.GetDotProduct(GetMomentum(), transform.up) > 0f);
			//Check if controller is sliding;
			bool _isSliding = mover.IsGrounded() && IsGroundTooSteep();

			//Grounded;
			if (CurrentControllerState == ControllerState.Grounded)
			{
				if (_isRising)
				{
					OnGroundContactLost();
					return ControllerState.Rising;
				}

				if (!mover.IsGrounded())
				{
					OnGroundContactLost();
					return ControllerState.Falling;
				}

				if (_isSliding)
				{
					OnGroundContactLost();
					return ControllerState.Sliding;
				}

				return ControllerState.Grounded;
			}

			//Falling;
			if (CurrentControllerState == ControllerState.Falling)
			{
				if (_isRising)
				{
					return ControllerState.Rising;
				}

				if (mover.IsGrounded() && !_isSliding)
				{
					OnGroundContactRegained();
					return ControllerState.Grounded;
				}

				if (_isSliding)
				{
					return ControllerState.Sliding;
				}

				return ControllerState.Falling;
			}

			//Sliding;
			if (CurrentControllerState == ControllerState.Sliding)
			{
				if (_isRising)
				{
					OnGroundContactLost();
					return ControllerState.Rising;
				}

				if (!mover.IsGrounded())
				{
					OnGroundContactLost();
					return ControllerState.Falling;
				}

				if (mover.IsGrounded() && !_isSliding)
				{
					OnGroundContactRegained();
					return ControllerState.Grounded;
				}

				return ControllerState.Sliding;
			}

			//Rising;
			if (CurrentControllerState == ControllerState.Rising)
			{
				if (!_isRising)
				{
					if (mover.IsGrounded() && !_isSliding)
					{
						OnGroundContactRegained();
						return ControllerState.Grounded;
					}

					if (_isSliding)
					{
						return ControllerState.Sliding;
					}

					if (!mover.IsGrounded())
					{
						return ControllerState.Falling;
					}
				}

				//If a ceiling detector has been attached to this gameobject, check for ceiling hits;
				if (ceilingDetector != null)
				{
					if (ceilingDetector.HitCeiling())
					{
						OnCeilingContact();
						return ControllerState.Falling;
					}
				}

				return ControllerState.Rising;
			}

			//Jumping;
			if (CurrentControllerState == ControllerState.Jumping)
			{
				//Check for jump timeout;
				if ((Time.time - currentJumpStartTime) > jumpDuration)
					return ControllerState.Rising;

				//Check if jump key was let go;
				if (jumpKeyWasLetGo)
					return ControllerState.Rising;

				//If a ceiling detector has been attached to this gameobject, check for ceiling hits;
				if (ceilingDetector != null)
				{
					if (ceilingDetector.HitCeiling())
					{
						OnCeilingContact();
						return ControllerState.Falling;
					}
				}

				return ControllerState.Jumping;
			}

			return ControllerState.Falling;
		}

		//Check if player has initiated a jump;
		void HandleJumping()
		{
			if (CurrentControllerState == ControllerState.Grounded)
			{
				if ((jumpKeyIsPressed == true || jumpKeyWasPressed) && !jumpInputIsLocked)
				{
					//Call events;
					OnGroundContactLost();
					OnJumpStart();

					CurrentControllerState = ControllerState.Jumping;
				}
			}
		}

		//Apply friction to both vertical and horizontal momentum based on 'friction' and 'gravity';
		//Handle movement in the air;
		//Handle sliding down steep slopes;
		void HandleMomentum()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if (useLocalMomentum)
				momentum = transform.localToWorldMatrix * momentum;

			Vector3 _verticalMomentum = Vector3.zero;
			Vector3 _horizontalMomentum = Vector3.zero;

			//Split momentum into vertical and horizontal components;
			if (momentum != Vector3.zero)
			{
				_verticalMomentum = VectorMath.ExtractDotVector(momentum, transform.up);
				_horizontalMomentum = momentum - _verticalMomentum;
			}

			//Add gravity to vertical momentum;
			_verticalMomentum -= transform.up * gravity * Time.deltaTime;

			//Remove any downward force if the controller is grounded;
			if (CurrentControllerState == ControllerState.Grounded &&
			    VectorMath.GetDotProduct(_verticalMomentum, transform.up) < 0f)
				_verticalMomentum = Vector3.zero;

			//Manipulate momentum to steer controller in the air (if controller is not grounded or sliding);
			if (!IsGrounded())
			{
				Vector3 _movementVelocity = CalculateMovementVelocity();

				//If controller has received additional momentum from somewhere else;
				if (_horizontalMomentum.magnitude > _movementVelocity.magnitude) //todo
				{
					//Prevent unwanted accumulation of speed in the direction of the current momentum;
					if (VectorMath.GetDotProduct(_movementVelocity, _horizontalMomentum.normalized) > 0f)
						_movementVelocity =
							VectorMath.RemoveDotVector(_movementVelocity, _horizontalMomentum.normalized);

					//Lower air control slightly with a multiplier to add some 'weight' to any momentum applied to the controller;
					float _airControlMultiplier = 0.25f;
					_horizontalMomentum += _movementVelocity * Time.deltaTime * airControlRate * _airControlMultiplier;
				}
				//If controller has not received additional momentum;
				else
				{
					//Clamp _horizontal velocity to prevent accumulation of speed;
					_horizontalMomentum += _movementVelocity * Time.deltaTime * airControlRate;
					_horizontalMomentum =
						Vector3.ClampMagnitude(_horizontalMomentum, _movementVelocity.magnitude); //todo
				}
			}

			//Steer controller on slopes;
			if (CurrentControllerState == ControllerState.Sliding)
			{
				//Calculate vector pointing away from slope;
				Vector3 _pointDownVector = Vector3.ProjectOnPlane(mover.GetGroundNormal(), transform.up).normalized;

				//Calculate movement velocity;
				Vector3 _slopeMovementVelocity = CalculateMovementVelocity();
				//Remove all velocity that is pointing up the slope;
				_slopeMovementVelocity = VectorMath.RemoveDotVector(_slopeMovementVelocity, _pointDownVector);

				//Add movement velocity to momentum;
				_horizontalMomentum += _slopeMovementVelocity * Time.fixedDeltaTime;
			}

			//Apply friction to horizontal momentum based on whether the controller is grounded;
			if (CurrentControllerState == ControllerState.Grounded)
				_horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(_horizontalMomentum, groundFriction,
					Time.deltaTime, Vector3.zero);
			else
				_horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(_horizontalMomentum, airFriction,
					Time.deltaTime, Vector3.zero);

			//Add horizontal and vertical momentum back together;
			momentum = _horizontalMomentum + _verticalMomentum;

			//Additional momentum calculations for sliding;
			if (CurrentControllerState == ControllerState.Sliding)
			{
				//Project the current momentum onto the current ground normal if the controller is sliding down a slope;
				momentum = Vector3.ProjectOnPlane(momentum, mover.GetGroundNormal());

				//Remove any upwards momentum when sliding;
				if (VectorMath.GetDotProduct(momentum, transform.up) > 0f)
					momentum = VectorMath.RemoveDotVector(momentum, transform.up);

				//Apply additional slide gravity;
				Vector3 _slideDirection = Vector3.ProjectOnPlane(-transform.up, mover.GetGroundNormal()).normalized;
				momentum += _slideDirection * slideGravity * Time.deltaTime;
			}

			//If controller is jumping, override vertical velocity with jumpSpeed;
			if (CurrentControllerState == ControllerState.Jumping)
			{
				momentum = VectorMath.RemoveDotVector(momentum, transform.up);
				momentum += transform.up * jumpSpeed;
			}

			if (useLocalMomentum)
				momentum = transform.worldToLocalMatrix * momentum;
		}

		//Events;

		//This function is called when the player has initiated a jump;
		void OnJumpStart()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if (useLocalMomentum)
				momentum = transform.localToWorldMatrix * momentum;

			//Add jump force to momentum;
			momentum += transform.up * jumpSpeed;

			//Set jump start time;
			currentJumpStartTime = Time.time;

			//Lock jump input until jump key is released again;
			jumpInputIsLocked = true;

			//Call event;
			OnJump(momentum);

			if (useLocalMomentum)
				momentum = transform.worldToLocalMatrix * momentum;
		}

		//This function is called when the controller has lost ground contact, i.e. is either falling or rising, or generally in the air;
		void OnGroundContactLost()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if (useLocalMomentum)
				momentum = transform.localToWorldMatrix * momentum;

			//Get current movement velocity;
			Vector3 _velocity = GetMovementVelocity();

			//Check if the controller has both momentum and a current movement velocity;
			if (_velocity.sqrMagnitude >= 0f && momentum.sqrMagnitude > 0f)
			{
				//Project momentum onto movement direction;
				Vector3 _projectedMomentum = Vector3.Project(momentum, _velocity.normalized);
				//Calculate dot product to determine whether momentum and movement are aligned;
				float _dot = VectorMath.GetDotProduct(_projectedMomentum.normalized, _velocity.normalized);

				//If current momentum is already pointing in the same direction as movement velocity,
				//Don't add further momentum (or limit movement velocity) to prevent unwanted speed accumulation;
				if (_projectedMomentum.sqrMagnitude >= _velocity.sqrMagnitude && _dot > 0f)
					_velocity = Vector3.zero;
				else if (_dot > 0f)
					_velocity -= _projectedMomentum;
			}

			//Add movement velocity to momentum;
			momentum += _velocity;

			if (useLocalMomentum)
				momentum = transform.worldToLocalMatrix * momentum;
		}

		//This function is called when the controller has landed on a surface after being in the air;
		void OnGroundContactRegained()
		{
			//Call 'OnLand' event;
			Vector3 _collisionVelocity = momentum;
			//If local momentum is used, transform momentum into world coordinates first;
			if (useLocalMomentum)
				_collisionVelocity = transform.localToWorldMatrix * _collisionVelocity;

			OnLand(_collisionVelocity);

		}

		//This function is called when the controller has collided with a ceiling while jumping or moving upwards;
		void OnCeilingContact()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if (useLocalMomentum)
				momentum = transform.localToWorldMatrix * momentum;

			//Remove all vertical parts of momentum;
			momentum = VectorMath.RemoveDotVector(momentum, transform.up);

			if (useLocalMomentum)
				momentum = transform.worldToLocalMatrix * momentum;
		}

		//Helper functions;

		//Returns 'true' if vertical momentum is above a small threshold;
		private bool IsRisingOrFalling()
		{
			//Calculate current vertical momentum;
			Vector3 _verticalMomentum = VectorMath.ExtractDotVector(GetMomentum(), transform.up);

			//Setup threshold to check against;
			//For most applications, a value of '0.001f' is recommended;
			float _limit = 0.001f;

			//Return true if vertical momentum is above '_limit';
			return (_verticalMomentum.magnitude > _limit);
		}

		//Returns true if angle between controller and ground normal is too big (> slope limit), i.e. ground is too steep;
		private bool IsGroundTooSteep()
		{
			if (!mover.IsGrounded())
				return true;

			return (Vector3.Angle(mover.GetGroundNormal(), transform.up) > slopeLimit);
		}

		//Getters;

		//Get last frame's velocity;
		public Vector3 GetVelocity()
		{
			return savedVelocity;
		}

		//Get last frame's movement velocity (momentum is ignored);
		public Vector3 GetMovementVelocity()
		{
			return savedMovementVelocity;
		}

		//Get current momentum;
		public Vector3 GetMomentum()
		{
			Vector3 _worldMomentum = momentum;
			if (useLocalMomentum)
				_worldMomentum = transform.localToWorldMatrix * momentum;

			return _worldMomentum;
		}

		//Returns 'true' if controller is grounded (or sliding down a slope);
		public bool IsGrounded()
		{
			return (CurrentControllerState == ControllerState.Grounded ||
			        CurrentControllerState == ControllerState.Sliding);
		}

		//Returns 'true' if controller is sliding;
		public bool IsSliding()
		{
			return (CurrentControllerState == ControllerState.Sliding);
		}

		//Add momentum to controller;
		public void AddMomentum(Vector3 _momentum)
		{
			if (useLocalMomentum)
				momentum = transform.localToWorldMatrix * momentum;

			momentum += _momentum;

			if (useLocalMomentum)
				momentum = transform.worldToLocalMatrix * momentum;
		}

		//Set controller momentum directly;
		public void SetMomentum(Vector3 _newMomentum)
		{
			if (useLocalMomentum)
				momentum = transform.worldToLocalMatrix * _newMomentum;
			else
				momentum = _newMomentum;
		}

		/// <summary>
		/// 强制朝向
		/// </summary>
		/// <param name="target"></param>
		public void ForceLookAt(Vector3 target)
		{
			Vector3 dir = target - unit.Position;
			dir.y = 0;
			unit.Rotation = Quaternion.LookRotation(dir, Vector3.up);
		}
	}
}