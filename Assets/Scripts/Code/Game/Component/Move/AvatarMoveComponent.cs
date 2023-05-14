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

	    //'AirFriction' determines how fast the controller loses its momentum while in the air;
	    //'GroundFriction' is used instead, if the controller is grounded;
	    public float airFriction = 0.5f;
	    public float groundFriction = 100f;

	    //Current momentum;
	    protected Vector3 momentum = Vector3.zero;

	    //Amount of downward gravity;
	    public float gravity = 20f;

	    [Tooltip("Whether to calculate and apply momentum relative to the controller's transform.")]
	    public bool useLocalMomentum = false;

	    //Enum describing basic controller states; 
	    public enum ControllerState
	    {
		    Grounded,
		    Falling
	    }
		
	    ControllerState currentControllerState = ControllerState.Falling;


	    public Transform cameraTransform;
	    
	    #endregion
	    
        private bool canMove = true;
        private bool canTurn = true;
        private FsmComponent FsmComponent => parent.GetComponent<FsmComponent>();

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
            if (direction != Vector3.zero)
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
			if(!canTurn) return;
			var lookDir = CalculateLookDirection();
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

			//Reset ceiling detector, if one is attached to this gameobject;
			if(ceilingDetector != null)
				ceilingDetector.ResetFlags();
		}
		
		protected virtual Vector3 CalculateLookDirection()
		{
			//If no character input script is attached to this object, return;
			if(characterInput == null)
				return Vector3.zero;

			Vector3 _velocity = Vector3.zero;

			//If no camera transform has been assigned, use the character's transform axes to calculate the movement direction;
			if(cameraTransform == null)
			{
				_velocity += tr.right * characterInput.Direction.x;
				_velocity += tr.forward * characterInput.Direction.z;
			}
			else
			{
				//If a camera transform has been assigned, use the assigned transform's axes for movement direction;
				//Project movement direction so movement stays parallel to the ground;
				_velocity += Vector3.ProjectOnPlane(cameraTransform.right, tr.up).normalized * characterInput.Direction.x;
				_velocity += Vector3.ProjectOnPlane(cameraTransform.forward, tr.up).normalized * characterInput.Direction.z;
			}
			
			_velocity.Normalize();
			return _velocity;
		}

		//Determine current controller state based on current momentuml and whether the controller is grounded (or not);
		//Handle state transitions;
		ControllerState DetermineControllerState()
		{
			//Grounded;
			if(currentControllerState == ControllerState.Grounded)
			{
				if(!mover.IsGrounded()){
					OnGroundContactLost();
					return ControllerState.Falling;
				}
				return ControllerState.Grounded;
			}

			//Falling;
			if(currentControllerState == ControllerState.Falling)
			{
				if(mover.IsGrounded()){
					return ControllerState.Grounded;
				}
				return ControllerState.Falling;
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

			//Apply friction to horizontal momentum based on whether the controller is grounded;
			if(currentControllerState == ControllerState.Grounded)
				_horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(_horizontalMomentum, groundFriction, deltaTime, Vector3.zero);
			else
				_horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(_horizontalMomentum, airFriction, deltaTime, Vector3.zero); 

			//Add horizontal and vertical momentum back together;
			momentum = _horizontalMomentum + _verticalMomentum;
			
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
			Vector3 _velocity = Vector3.zero;

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
		
		//Returns 'true' if controller is grounded (or sliding down a slope);
		public bool IsGrounded()
		{
			return currentControllerState == ControllerState.Grounded;
		}
    }
}
