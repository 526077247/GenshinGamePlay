using UnityEngine;
using CMF;

namespace TaoTie
{
    public class AvatarMoveComponent : Component, IComponent,IUpdateComponent
    {
	    #region Param

	    //References to attached components;
	    protected Transform tr;
	    protected Mover mover;
	    public CharacterKeyboardInput characterInput;
	    protected CeilingDetector ceilingDetector;

	    //How fast the controller can change direction while in the air;
	    //Higher values result in more air control;
	    public float airControlRate = 2f;

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
	    public float gravity = 20f;
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
		    Rising
	    }
		
	    ControllerState currentControllerState = ControllerState.Falling;


	    public Transform cameraTransform;
	    
	    

	    #endregion
	    
        private bool canMove = true;
        private bool canTurn = true;
        private FsmComponent FsmComponent => parent.GetComponent<FsmComponent>();
        private NumericComponent NumericComponent => parent.GetComponent<NumericComponent>();

        private Unit unit => parent as Unit;
        public void Init()
        {
	        characterInput = new CharacterKeyboardInput();
            canMove = FsmComponent.DefaultFsm.currentState.CanMove;
            canTurn = FsmComponent.DefaultFsm.currentState.CanTurn;
            Messager.Instance.AddListener<bool>(Id, MessageId.SetCanMove, SetCanMove);
            Messager.Instance.AddListener<bool>(Id, MessageId.SetCanTurn, SetCanTurn);
            InitAsync().Coroutine();
        }

        private async ETTask InitAsync()
        {
	        var gh = parent.GetComponent<GameObjectHolderComponent>();
	        await gh.WaitLoadGameObjectOver();
	        mover = gh.EntityView.GetComponent<Mover>();
	        tr = gh.EntityView;
	        ceilingDetector = gh.EntityView.GetComponent<CeilingDetector>();
	        cameraTransform = CameraManager.Instance.MainCamera().transform;
        }

        public void Destroy()
        {
            Messager.Instance.RemoveListener<bool>(Id, MessageId.SetCanMove, SetCanMove);
            Messager.Instance.RemoveListener<bool>(Id, MessageId.SetCanTurn, SetCanTurn);
        }

        public void TryMove(Vector3 direction)
        {
            if (canMove)
            {
                characterInput.Direction = direction;
            }
            else
            {
	            characterInput.Direction = Vector3.zero;
            }
            if (characterInput.Direction != Vector3.zero)
            {
	            MoveStart();
            }
            else
            {
	            MoveStop();
            }
        }

        private void SetCanMove(bool canMove)
        {
            this.canMove = canMove;
        }

        private void SetCanTurn(bool canTurn)
        {
            this.canTurn = canTurn;
        }

        private void MoveStart()
        {
	        characterInput.MotionFlag = MotionFlag.Run;
            FsmComponent.SetData(FSMConst.MotionFlag, 2);
        }

        private void MoveStop()
        {
	        characterInput.MotionFlag = MotionFlag.Idle;
            FsmComponent.SetData(FSMConst.MotionFlag, 0);
        }

        public void Update()
		{
			if(tr == null) return;
			ControllerUpdate();
			HandlerForward();
			unit.SyncViewPosition(tr.position);
		}
		void HandlerForward()
		{
			var lookDir = CalculateMovementDirection();
			if (lookDir != Vector3.zero)
			{
				Vector3 dir;
				var angle = Vector3.Angle(lookDir, tr.forward);
				if (angle > 3)
				{
					var flag = Vector3.Cross(lookDir, tr.forward);
					var temp = (flag.y > 0 ? -1 : 1) * 360 * GameTimerManager.Instance.GetDeltaTime() / 1000f;
					if (Mathf.Abs(temp - angle)<0) temp = angle;
					dir = Quaternion.Euler(0, temp, 0) * tr.forward;
				}
				else
				{
					dir = lookDir;
				}
				Vector3 _upDirection = tr.up;
				unit.Rotation = Quaternion.LookRotation(dir, _upDirection);
			}
		}

		//Update controller;
		//This function must be called every fixed update, in order for the controller to work correctly;
		void ControllerUpdate()
		{
			//Check if mover is grounded;
			mover.CheckForGround();

			//Determine controller state;
			currentControllerState = DetermineControllerState();

			//Apply friction and gravity to 'momentum';
			HandleMomentum();

			//Calculate movement velocity;
			Vector3 _velocity = Vector3.zero;
			if(currentControllerState == ControllerState.Grounded)
				_velocity = CalculateMovementVelocity();
			
			//If local momentum is used, transform momentum into world space first;
			Vector3 _worldMomentum = momentum;
			if(useLocalMomentum)
				_worldMomentum = tr.localToWorldMatrix * momentum;

			//Add current momentum to velocity;
			_velocity += _worldMomentum;
			
			//If player is grounded or sliding on a slope, extend mover's sensor range;
			//This enables the player to walk up/down stairs and slopes without losing ground contact;
			mover.SetExtendSensorRange(IsGrounded());

			//Set mover velocity;		
			mover.SetVelocity(_velocity);

			//Store velocity for next frame;
			savedVelocity = _velocity;
		
			//Save controller movement velocity;
			savedMovementVelocity = CalculateMovementVelocity();
			
			//Reset ceiling detector, if one is attached to this gameobject;
			if(ceilingDetector != null)
				ceilingDetector.ResetFlags();
		}

		//Calculate and return movement direction based on player input;
		//This function can be overridden by inheriting scripts to implement different player controls;
		protected virtual Vector3 CalculateMovementDirection()
		{
			//If no character input script is attached to this object, return;
			if(characterInput == null)
				return Vector3.zero;

			Vector3 _velocity = Vector3.zero;

			//If no camera transform has been assigned, use the character's transform axes to calculate the movement direction;
			if(cameraTransform == null)
			{
				_velocity += tr.right * characterInput.GetHorizontalMovementInput();
				_velocity += tr.forward * characterInput.GetVerticalMovementInput();
			}
			else
			{
				//If a camera transform has been assigned, use the assigned transform's axes for movement direction;
				//Project movement direction so movement stays parallel to the ground;
				_velocity += Vector3.ProjectOnPlane(cameraTransform.right, tr.up).normalized * characterInput.GetHorizontalMovementInput();
				_velocity += Vector3.ProjectOnPlane(cameraTransform.forward, tr.up).normalized * characterInput.GetVerticalMovementInput();
			}

			//If necessary, clamp movement vector to magnitude of 1f;
			if(_velocity.magnitude > 1f)
				_velocity.Normalize();

			return _velocity;
		}

		//Calculate and return movement velocity based on player input, controller state, ground normal [...];
		protected virtual Vector3 CalculateMovementVelocity()
		{
			if (characterInput.MotionFlag == MotionFlag.Run || characterInput.MotionFlag == MotionFlag.Walk)
			{
				//Calculate (normalized) movement direction;
				Vector3 _velocity = tr.forward;

				//Multiply (normalized) velocity with movement speed;
				_velocity *= NumericComponent.GetAsFloat(NumericType.Speed);
				return _velocity;
			}

			return Vector3.zero;
		}

		//Determine current controller state based on current momentum and whether the controller is grounded (or not);
		//Handle state transitions;
		ControllerState DetermineControllerState()
		{
			//Check if vertical momentum is pointing upwards;
			bool _isRising = IsRisingOrFalling() && (VectorMath.GetDotProduct(GetMomentum(), tr.up) > 0f);
			//Check if controller is sliding;
			bool _isSliding = mover.IsGrounded() && IsGroundTooSteep();
			
			//Grounded;
			if(currentControllerState == ControllerState.Grounded)
			{
				if(_isRising){
					OnGroundContactLost();
					return ControllerState.Rising;
				}
				if(!mover.IsGrounded()){
					OnGroundContactLost();
					return ControllerState.Falling;
				}
				if(_isSliding){
					OnGroundContactLost();
					return ControllerState.Sliding;
				}
				return ControllerState.Grounded;
			}

			//Falling;
			if(currentControllerState == ControllerState.Falling)
			{
				if(_isRising){
					return ControllerState.Rising;
				}
				if(mover.IsGrounded() && !_isSliding){
					return ControllerState.Grounded;
				}
				if(_isSliding){
					return ControllerState.Sliding;
				}
				return ControllerState.Falling;
			}
			
			//Sliding;
			if(currentControllerState == ControllerState.Sliding)
			{	
				if(_isRising){
					OnGroundContactLost();
					return ControllerState.Rising;
				}
				if(!mover.IsGrounded()){
					OnGroundContactLost();
					return ControllerState.Falling;
				}
				if(mover.IsGrounded() && !_isSliding){
					return ControllerState.Grounded;
				}
				return ControllerState.Sliding;
			}

			//Rising;
			if(currentControllerState == ControllerState.Rising)
			{
				if(!_isRising){
					if(mover.IsGrounded() && !_isSliding){
						return ControllerState.Grounded;
					}
					if(_isSliding){
						return ControllerState.Sliding;
					}
					if(!mover.IsGrounded()){
						return ControllerState.Falling;
					}
				}

				//If a ceiling detector has been attached to this gameobject, check for ceiling hits;
				if(ceilingDetector != null)
				{
					if(ceilingDetector.HitCeiling())
					{
						OnCeilingContact();
						return ControllerState.Falling;
					}
				}
				return ControllerState.Rising;
			}

			return ControllerState.Falling;
		}
		
        //Apply friction to both vertical and horizontal momentum based on 'friction' and 'gravity';
		//Handle movement in the air;
        //Handle sliding down steep slopes;
        void HandleMomentum()
        {
	        float deltaTime = GameTimerManager.Instance.GetDeltaTime() / 1000f;
			//If local momentum is used, transform momentum into world coordinates first;
			if(useLocalMomentum)
				momentum = tr.localToWorldMatrix * momentum;

			Vector3 _verticalMomentum = Vector3.zero;
			Vector3 _horizontalMomentum = Vector3.zero;

			//Split momentum into vertical and horizontal components;
			if(momentum != Vector3.zero)
			{
				_verticalMomentum = VectorMath.ExtractDotVector(momentum, tr.up);
				_horizontalMomentum = momentum - _verticalMomentum;
			}

			//Add gravity to vertical momentum;
			_verticalMomentum -= tr.up * gravity * deltaTime;

			//Remove any downward force if the controller is grounded;
			if(currentControllerState == ControllerState.Grounded && VectorMath.GetDotProduct(_verticalMomentum, tr.up) < 0f)
				_verticalMomentum = Vector3.zero;

			var speed= NumericComponent.GetAsFloat(NumericType.Speed);
			//Manipulate momentum to steer controller in the air (if controller is not grounded or sliding);
			if(!IsGrounded())
			{
				Vector3 _movementVelocity = CalculateMovementVelocity();

				//If controller has received additional momentum from somewhere else;
				if(_horizontalMomentum.magnitude > speed)
				{
					//Prevent unwanted accumulation of speed in the direction of the current momentum;
					if(VectorMath.GetDotProduct(_movementVelocity, _horizontalMomentum.normalized) > 0f)
						_movementVelocity = VectorMath.RemoveDotVector(_movementVelocity, _horizontalMomentum.normalized);
					
					//Lower air control slightly with a multiplier to add some 'weight' to any momentum applied to the controller;
					float _airControlMultiplier = 0.25f;
					_horizontalMomentum += _movementVelocity * deltaTime * airControlRate * _airControlMultiplier;
				}
				//If controller has not received additional momentum;
				else
				{
					//Clamp _horizontal velocity to prevent accumulation of speed;
					_horizontalMomentum += _movementVelocity * deltaTime * airControlRate;
					_horizontalMomentum = Vector3.ClampMagnitude(_horizontalMomentum, speed);
				}
			}

			//Steer controller on slopes;
			if(currentControllerState == ControllerState.Sliding)
			{
				//Calculate vector pointing away from slope;
				Vector3 _pointDownVector = Vector3.ProjectOnPlane(mover.GetGroundNormal(), tr.up).normalized;

				//Calculate movement velocity;
				Vector3 _slopeMovementVelocity = CalculateMovementVelocity();
				//Remove all velocity that is pointing up the slope;
				_slopeMovementVelocity = VectorMath.RemoveDotVector(_slopeMovementVelocity, _pointDownVector);

				//Add movement velocity to momentum;
				_horizontalMomentum += _slopeMovementVelocity * deltaTime;
			}

			//Apply friction to horizontal momentum based on whether the controller is grounded;
			if(currentControllerState == ControllerState.Grounded)
				_horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(_horizontalMomentum, groundFriction, deltaTime, Vector3.zero);
			else
				_horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(_horizontalMomentum, airFriction, deltaTime, Vector3.zero); 

			//Add horizontal and vertical momentum back together;
			momentum = _horizontalMomentum + _verticalMomentum;

			//Additional momentum calculations for sliding;
			if(currentControllerState == ControllerState.Sliding)
			{
				//Project the current momentum onto the current ground normal if the controller is sliding down a slope;
				momentum = Vector3.ProjectOnPlane(momentum, mover.GetGroundNormal());

				//Remove any upwards momentum when sliding;
				if(VectorMath.GetDotProduct(momentum, tr.up) > 0f)
					momentum = VectorMath.RemoveDotVector(momentum, tr.up);

				//Apply additional slide gravity;
				Vector3 _slideDirection = Vector3.ProjectOnPlane(-tr.up, mover.GetGroundNormal()).normalized;
				momentum += _slideDirection * slideGravity * deltaTime;
			}
			
			if(useLocalMomentum)
				momentum = tr.worldToLocalMatrix * momentum;
		}
        
		//This function is called when the controller has lost ground contact, i.e. is either falling or rising, or generally in the air;
		void OnGroundContactLost()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if(useLocalMomentum)
				momentum = tr.localToWorldMatrix * momentum;

			//Get current movement velocity;
			Vector3 _velocity = GetMovementVelocity();

			//Check if the controller has both momentum and a current movement velocity;
			if(_velocity.sqrMagnitude >= 0f && momentum.sqrMagnitude > 0f)
			{
				//Project momentum onto movement direction;
				Vector3 _projectedMomentum = Vector3.Project(momentum, _velocity.normalized);
				//Calculate dot product to determine whether momentum and movement are aligned;
				float _dot = VectorMath.GetDotProduct(_projectedMomentum.normalized, _velocity.normalized);

				//If current momentum is already pointing in the same direction as movement velocity,
				//Don't add further momentum (or limit movement velocity) to prevent unwanted speed accumulation;
				if(_projectedMomentum.sqrMagnitude >= _velocity.sqrMagnitude && _dot > 0f)
					_velocity = Vector3.zero;
				else if(_dot > 0f)
					_velocity -= _projectedMomentum;	
			}

			//Add movement velocity to momentum;
			momentum += _velocity;

			if(useLocalMomentum)
				momentum = tr.worldToLocalMatrix * momentum;
		}

		//This function is called when the controller has collided with a ceiling while jumping or moving upwards;
		void OnCeilingContact()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if(useLocalMomentum)
				momentum = tr.localToWorldMatrix * momentum;

			//Remove all vertical parts of momentum;
			momentum = VectorMath.RemoveDotVector(momentum, tr.up);

			if(useLocalMomentum)
				momentum = tr.worldToLocalMatrix * momentum;
		}

		//Helper functions;

		//Returns 'true' if vertical momentum is above a small threshold;
		private bool IsRisingOrFalling()
		{
			//Calculate current vertical momentum;
			Vector3 _verticalMomentum = VectorMath.ExtractDotVector(GetMomentum(), tr.up);

			//Setup threshold to check against;
			//For most applications, a value of '0.001f' is recommended;
			float _limit = 0.001f;

			//Return true if vertical momentum is above '_limit';
			return(_verticalMomentum.magnitude > _limit);
		}

		//Returns true if angle between controller and ground normal is too big (> slope limit), i.e. ground is too steep;
		private bool IsGroundTooSteep()
		{
			if(!mover.IsGrounded())
				return true;

			return (Vector3.Angle(mover.GetGroundNormal(), tr.up) > slopeLimit);
		}

		//Getters;

		//Get last frame's velocity;
		public Vector3 GetVelocity ()
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
			if(useLocalMomentum)
				_worldMomentum = tr.localToWorldMatrix * momentum;

			return _worldMomentum;
		}

		//Returns 'true' if controller is grounded (or sliding down a slope);
		public bool IsGrounded()
		{
			return(currentControllerState == ControllerState.Grounded || currentControllerState == ControllerState.Sliding);
		}

		//Returns 'true' if controller is sliding;
		public bool IsSliding()
		{
			return(currentControllerState == ControllerState.Sliding);
		}

		//Add momentum to controller;
		public void AddMomentum (Vector3 _momentum)
		{
			if(useLocalMomentum)
				momentum = tr.localToWorldMatrix * momentum;

			momentum += _momentum;	

			if(useLocalMomentum)
				momentum = tr.worldToLocalMatrix * momentum;
		}

		//Set controller momentum directly;
		public void SetMomentum(Vector3 _newMomentum)
		{
			if(useLocalMomentum)
				momentum = tr.worldToLocalMatrix * _newMomentum;
			else
				momentum = _newMomentum;
		}
    }
}
