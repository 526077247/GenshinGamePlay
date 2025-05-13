using UnityEngine;

namespace TaoTie
{

	public partial class AnimatorMoveComponent : MoveComponent<ConfigAnimatorMove>, IFixedUpdate
	{
		public override bool useAnimMove => true;

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

		/// <summary>
		/// How fast the character will slide down steep slopes.
		/// </summary>
		public float slideGravity = 5f;

		//Acceptable slope angle limit;
		public float slopeLimit = 80f;

		/// <summary>
		/// Whether to calculate and apply momentum relative to the controller's transform.
		/// </summary>
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

		public ControllerState CurrentControllerState { get; private set; } = ControllerState.Falling;
		
		protected override void InitInternal()
		{
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
		}

		protected override void DestroyInternal()
		{
			if (mover != null)
			{
				mover.OnAnimatorMoveEvt = null;
				mover.enabled = false;
				mover = null;
			}
			transform = null;
		}

		protected override void UpdateInternal()
		{
			
		}
		public void FixedUpdate()
		{
			if (transform == null || mover == null) return;
			ControllerUpdate();
			HandlerForward();
			HandleJumpKeyInput();
			SceneEntity.SyncViewPosition(transform.position);
		}

		void HandlerForward()
		{
			// if(!canTurn) return;
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
					var temp = (flag.y > 0 ? -1 : 1) * CharacterInput.RotateSpeed * deltaTime;
					if (Mathf.Abs(temp - angle) <= 0.5f) temp = angle;
					dir = Quaternion.Euler(0, temp, 0) * transform.forward;
				}
				else
				{
					dir = lookDir;
				}

				SceneEntity.Rotation = Quaternion.LookRotation(dir, Vector3.up);
			}
		}

		protected Vector3 CalculateLookDirection()
		{
			if (CharacterInput == null || CharacterInput.FaceDirection == Vector3.zero)
				return Vector3.zero;

			Vector3 v = Vector3.zero;
			
			var faceRight = Quaternion.Euler(0, 90, 0) * CharacterInput.FaceDirection;
			v += Vector3.ProjectOnPlane(faceRight, transform.up).normalized * CharacterInput.Direction.x;
			v += Vector3.ProjectOnPlane(CharacterInput.FaceDirection, transform.up).normalized * CharacterInput.Direction.z;

			v.Normalize();
			return v;
		}

		//Handle jump booleans for later use in FixedUpdate;
		void HandleJumpKeyInput()
		{
			bool newJumpKeyPressedState = IsJumpKeyPressed();

			if (jumpKeyIsPressed == false && newJumpKeyPressedState == true)
				jumpKeyWasPressed = true;

			if (jumpKeyIsPressed == true && newJumpKeyPressedState == false)
			{
				jumpKeyWasLetGo = true;
				jumpInputIsLocked = false;
			}

			jumpKeyIsPressed = newJumpKeyPressedState;
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

			Vector3 velocity = Vector3.zero;

			velocity += transform.right * CharacterInput.GetHorizontalMovementInput();
			velocity += transform.forward * CharacterInput.GetVerticalMovementInput();

			return velocity;
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
			bool isRising = IsRisingOrFalling() && (VectorMath.GetDotProduct(GetMomentum(), transform.up) > 0f);
			//Check if controller is sliding;
			bool isSliding = mover.IsGrounded() && IsGroundTooSteep();

			//Grounded;
			if (CurrentControllerState == ControllerState.Grounded)
			{
				if (isRising)
				{
					OnGroundContactLost();
					return ControllerState.Rising;
				}

				if (!mover.IsGrounded())
				{
					OnGroundContactLost();
					return ControllerState.Falling;
				}

				if (isSliding)
				{
					OnGroundContactLost();
					return ControllerState.Sliding;
				}

				return ControllerState.Grounded;
			}

			//Falling;
			if (CurrentControllerState == ControllerState.Falling)
			{
				if (isRising)
				{
					return ControllerState.Rising;
				}

				if (mover.IsGrounded() && !isSliding)
				{
					OnGroundContactRegained();
					return ControllerState.Grounded;
				}

				if (isSliding)
				{
					return ControllerState.Sliding;
				}

				return ControllerState.Falling;
			}

			//Sliding;
			if (CurrentControllerState == ControllerState.Sliding)
			{
				if (isRising)
				{
					OnGroundContactLost();
					return ControllerState.Rising;
				}

				if (!mover.IsGrounded())
				{
					OnGroundContactLost();
					return ControllerState.Falling;
				}

				if (mover.IsGrounded() && !isSliding)
				{
					OnGroundContactRegained();
					return ControllerState.Grounded;
				}

				return ControllerState.Sliding;
			}

			//Rising;
			if (CurrentControllerState == ControllerState.Rising)
			{
				if (!isRising)
				{
					if (mover.IsGrounded() && !isSliding)
					{
						OnGroundContactRegained();
						return ControllerState.Grounded;
					}

					if (isSliding)
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
				if ((jumpKeyIsPressed || jumpKeyWasPressed) && !jumpInputIsLocked)
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

			Vector3 verticalMomentum = Vector3.zero;
			Vector3 horizontalMomentum = Vector3.zero;

			//Split momentum into vertical and horizontal components;
			if (momentum != Vector3.zero)
			{
				verticalMomentum = VectorMath.ExtractDotVector(momentum, transform.up);
				horizontalMomentum = momentum - verticalMomentum;
			}

			//Add gravity to vertical momentum;
			verticalMomentum -= transform.up * gravity * Time.deltaTime;

			//Remove any downward force if the controller is grounded;
			if (CurrentControllerState == ControllerState.Grounded &&
			    VectorMath.GetDotProduct(verticalMomentum, transform.up) < 0f)
				verticalMomentum = Vector3.zero;

			//Manipulate momentum to steer controller in the air (if controller is not grounded or sliding);
			if (!IsGrounded())
			{
				Vector3 movementVelocity = CalculateMovementVelocity();

				//If controller has received additional momentum from somewhere else;
				if (horizontalMomentum.magnitude > movementVelocity.magnitude) //todo
				{
					//Prevent unwanted accumulation of speed in the direction of the current momentum;
					if (VectorMath.GetDotProduct(movementVelocity, horizontalMomentum.normalized) > 0f)
						movementVelocity =
							VectorMath.RemoveDotVector(movementVelocity, horizontalMomentum.normalized);

					//Lower air control slightly with a multiplier to add some 'weight' to any momentum applied to the controller;
					float airControlMultiplier = 0.25f;
					horizontalMomentum += movementVelocity * Time.deltaTime * airControlRate * airControlMultiplier;
				}
				//If controller has not received additional momentum;
				else
				{
					//Clamp horizontal velocity to prevent accumulation of speed;
					horizontalMomentum += movementVelocity * Time.deltaTime * airControlRate;
					horizontalMomentum =
						Vector3.ClampMagnitude(horizontalMomentum, movementVelocity.magnitude); //todo
				}
			}

			//Steer controller on slopes;
			if (CurrentControllerState == ControllerState.Sliding)
			{
				//Calculate vector pointing away from slope;
				Vector3 pointDownVector = Vector3.ProjectOnPlane(mover.GetGroundNormal(), transform.up).normalized;

				//Calculate movement velocity;
				Vector3 slopeMovementVelocity = CalculateMovementVelocity();
				//Remove all velocity that is pointing up the slope;
				slopeMovementVelocity = VectorMath.RemoveDotVector(slopeMovementVelocity, pointDownVector);

				//Add movement velocity to momentum;
				horizontalMomentum += slopeMovementVelocity * Time.fixedDeltaTime;
			}

			//Apply friction to horizontal momentum based on whether the controller is grounded;
			if (CurrentControllerState == ControllerState.Grounded)
				horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(horizontalMomentum, groundFriction,
					Time.deltaTime, Vector3.zero);
			else
				horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(horizontalMomentum, airFriction,
					Time.deltaTime, Vector3.zero);

			//Add horizontal and vertical momentum back together;
			momentum = horizontalMomentum + verticalMomentum;

			//Additional momentum calculations for sliding;
			if (CurrentControllerState == ControllerState.Sliding)
			{
				//Project the current momentum onto the current ground normal if the controller is sliding down a slope;
				momentum = Vector3.ProjectOnPlane(momentum, mover.GetGroundNormal());

				//Remove any upwards momentum when sliding;
				if (VectorMath.GetDotProduct(momentum, transform.up) > 0f)
					momentum = VectorMath.RemoveDotVector(momentum, transform.up);

				//Apply additional slide gravity;
				Vector3 slideDirection = Vector3.ProjectOnPlane(-transform.up, mover.GetGroundNormal()).normalized;
				momentum += slideDirection * slideGravity * Time.deltaTime;
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
			Vector3 velocity = GetMovementVelocity();

			//Check if the controller has both momentum and a current movement velocity;
			if (velocity.sqrMagnitude >= 0f && momentum.sqrMagnitude > 0f)
			{
				//Project momentum onto movement direction;
				Vector3 projectedMomentum = Vector3.Project(momentum, velocity.normalized);
				//Calculate dot product to determine whether momentum and movement are aligned;
				float dot = VectorMath.GetDotProduct(projectedMomentum.normalized, velocity.normalized);

				//If current momentum is already pointing in the same direction as movement velocity,
				//Don't add further momentum (or limit movement velocity) to prevent unwanted speed accumulation;
				if (projectedMomentum.sqrMagnitude >= velocity.sqrMagnitude && dot > 0f)
					velocity = Vector3.zero;
				else if (dot > 0f)
					velocity -= projectedMomentum;
			}

			//Add movement velocity to momentum;
			momentum += velocity;

			if (useLocalMomentum)
				momentum = transform.worldToLocalMatrix * momentum;
		}

		//This function is called when the controller has landed on a surface after being in the air;
		void OnGroundContactRegained()
		{
			//Call 'OnLand' event;
			Vector3 collisionVelocity = momentum;
			//If local momentum is used, transform momentum into world coordinates first;
			if (useLocalMomentum)
				collisionVelocity = transform.localToWorldMatrix * collisionVelocity;

			OnLand(collisionVelocity);

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
		
		//Returns 'true' if vertical momentum is above a small threshold;
		private bool IsRisingOrFalling()
		{
			//Calculate current vertical momentum;
			Vector3 verticalMomentum = VectorMath.ExtractDotVector(GetMomentum(), transform.up);

			//Setup threshold to check against;
			//For most applications, a value of '0.001f' is recommended;
			float limit = 0.001f;

			//Return true if vertical momentum is above 'limit';
			return (verticalMomentum.magnitude > limit);
		}

		//Returns true if angle between controller and ground normal is too big (> slope limit), i.e. ground is too steep;
		private bool IsGroundTooSteep()
		{
			if (!mover.IsGrounded())
				return true;

			return (Vector3.Angle(mover.GetGroundNormal(), transform.up) > slopeLimit);
		}
		
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
			Vector3 worldMomentum = momentum;
			if (useLocalMomentum)
				worldMomentum = transform.localToWorldMatrix * momentum;

			return worldMomentum;
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
		public void AddMomentum(Vector3 momentum)
		{
			if (useLocalMomentum)
				momentum = transform.localToWorldMatrix * momentum;

			momentum += momentum;

			if (useLocalMomentum)
				momentum = transform.worldToLocalMatrix * momentum;
		}

		//Set controller momentum directly;
		public void SetMomentum(Vector3 newMomentum)
		{
			if (useLocalMomentum)
				momentum = transform.worldToLocalMatrix * newMomentum;
			else
				momentum = newMomentum;
		}
		
	}
}