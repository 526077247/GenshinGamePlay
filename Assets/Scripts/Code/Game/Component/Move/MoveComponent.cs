using UnityEngine;
using CMF;

namespace TaoTie
{
    public partial class MoveComponent : Component, IComponent,IUpdate
    {
	    #region Param

	    //References to attached components;
	    private Transform transform;
	    private Mover mover;
	    private MoveInput moveInput;

	    //'AirFriction' determines how fast the controller loses its momentum while in the air;
	    //'GroundFriction' is used instead, if the controller is grounded;
	    private float airFriction = 0.5f;
	    private float groundFriction = 100f;

	    //Current momentum;
	    private Vector3 momentum = Vector3.zero;

	    //Amount of downward gravity;
	    private float gravity = 20f;

	    //Whether to calculate and apply momentum relative to the controller's transform.
	    private bool useLocalMomentum = true;

	    //Enum describing basic controller states; 
	    private enum ControllerState
	    {
		    Grounded,
		    Falling
	    }
		
	    ControllerState currentControllerState = ControllerState.Falling;


	    private Transform cameraTransform;
	    public float RotateSpeed = 180;
	    
	    #endregion
	    
        private bool canMove = true;
        private bool canTurn = true;
        private FsmComponent FsmComponent => parent.GetComponent<FsmComponent>();

        private Unit unit => parent as Unit;
        public void Init()
        {
	        currentControllerState = ControllerState.Falling;
	        RotateSpeed = 180;
	        momentum = Vector3.zero;
	        moveInput = new MoveInput();
            canMove = FsmComponent.DefaultFsm.CurrentState.CanMove;
            canTurn = FsmComponent.DefaultFsm.CurrentState.CanTurn;
            Messager.Instance.AddListener<bool>(Id, MessageId.SetCanMove, SetCanMove);
            Messager.Instance.AddListener<bool>(Id, MessageId.SetCanTurn, SetCanTurn);
            InitAsync().Coroutine();
        }

        private async ETTask InitAsync()
        {
	        var gh = parent.GetComponent<GameObjectHolderComponent>();
	        await gh.WaitLoadGameObjectOver();
	        if(gh.IsDispose) return;
	        mover = gh.EntityView.GetComponent<Mover>();
	        if (mover != null) mover.OnAnimatorMoveEvt = OnAnimatorMove;
	        transform = gh.EntityView;
	        cameraTransform = CameraManager.Instance.MainCamera().transform;
        }

        public void Destroy()
        {
	        if (mover != null)
	        {
		        mover.OnAnimatorMoveEvt = null;
		        mover = null;
	        }

	        transform = null;
	        cameraTransform = null;
	        moveInput = null;
            Messager.Instance.RemoveListener<bool>(Id, MessageId.SetCanMove, SetCanMove);
            Messager.Instance.RemoveListener<bool>(Id, MessageId.SetCanTurn, SetCanTurn);
        }

        public void TryMove(Vector3 direction, MotionFlag flag, MotionDirection mDirection)
        {
            if (canMove)
            {
                moveInput.Direction = direction;
            }
            else
            {
	            moveInput.Direction = Vector3.zero;
            }
            if (direction != Vector3.zero)
            {
	            MoveStart(flag, mDirection);
            }
            else
            {
	            MoveStop();
            }
        }
        public void TryMove(Vector3 direction)
        {
	        if (canMove)
	        {
		        moveInput.Direction = direction;
	        }
	        else
	        {
		        moveInput.Direction = Vector3.zero;
	        }
	        if (direction != Vector3.zero)
	        {
		        MoveStart(moveInput.MotionFlag, moveInput.MotionDirection);
	        }
	        else
	        {
		        MoveStop();
	        }
        }
        private void SetCanMove(bool canMove)
        {
	        if(IsDispose) return;
            this.canMove = canMove;
        }

        private void SetCanTurn(bool canTurn)
        {
	        if(IsDispose) return;
            this.canTurn = canTurn;
        }

        private void MoveStart(MotionFlag flag, MotionDirection mDirection)
        {
	        moveInput.MotionFlag = flag;
	        moveInput.MotionDirection = mDirection;
            FsmComponent.SetData(FSMConst.MotionFlag, (int)flag);
            if(FsmComponent.KeyExist(FSMConst.MotionDirection))
				FsmComponent.SetData(FSMConst.MotionDirection, (int)mDirection);
        }

        private void MoveStop()
        {
	        RotateSpeed = 180;
	        moveInput.MotionFlag = MotionFlag.Idle;
	        moveInput.MotionDirection = MotionDirection.Idle;
            FsmComponent.SetData(FSMConst.MotionFlag, 0);
            if(FsmComponent.KeyExist(FSMConst.MotionDirection))
				FsmComponent.SetData(FSMConst.MotionDirection, 0);
        }

        public void Update()
		{
			if(transform == null || mover==null) return;
			ControllerUpdate();
			HandlerForward();
			unit.SyncViewPosition(transform.position);
		}
		void HandlerForward()
		{
			if(!canTurn) return;
			if(!(parent is Avatar)) return;
			var lookDir = CalculateLookDirection();
			if (lookDir != Vector3.zero)
			{
				Vector3 dir;
				var angle = Vector3.Angle(lookDir, transform.forward);
				if (angle > 3)
				{
					var flag = Vector3.Cross(lookDir, transform.forward);
					var temp = (flag.y > 0 ? -1 : 1) * RotateSpeed * GameTimerManager.Instance.GetDeltaTime() / 500f;
					if (Mathf.Abs(temp - angle)<0) temp = angle;
					dir = Quaternion.Euler(0, temp, 0) * transform.forward;
				}
				else
				{
					dir = lookDir;
				}
				unit.Rotation = Quaternion.LookRotation(dir, Vector3.up);
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
			Vector3 v = Vector3.zero;

			//If local momentum is used, transform momentum into world space first;
			Vector3 worldMomentum = momentum;
			if(useLocalMomentum)
				worldMomentum = transform.localToWorldMatrix * momentum;

			//Add current momentum to velocity;
			v += worldMomentum;
			
			//If player is grounded or sliding on a slope, extend mover's sensor range;
			//This enables the player to walk up/down stairs and slopes without losing ground contact;
			mover.SetExtendSensorRange(IsGrounded());

			//Set mover velocity;		
			mover.SetVelocity(v);
		}
		
		protected virtual Vector3 CalculateLookDirection()
		{
			//If no character input script is attached to this object, return;
			if(moveInput == null)
				return Vector3.zero;

			Vector3 v = Vector3.zero;

			//If no camera transform has been assigned, use the character's transform axes to calculate the movement direction;
			if(cameraTransform == null)
			{
				v += transform.right * moveInput.Direction.x;
				v += transform.forward * moveInput.Direction.z;
			}
			else
			{
				//If a camera transform has been assigned, use the assigned transform's axes for movement direction;
				//Project movement direction so movement stays parallel to the ground;
				v += Vector3.ProjectOnPlane(cameraTransform.right, transform.up).normalized * moveInput.Direction.x;
				v += Vector3.ProjectOnPlane(cameraTransform.forward, transform.up).normalized * moveInput.Direction.z;
			}
			
			v.Normalize();
			return v;
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
				momentum = transform.localToWorldMatrix * momentum;

			Vector3 vm = Vector3.zero;
			Vector3 hm = Vector3.zero;

			//Split momentum into vertical and horizontal components;
			if(momentum != Vector3.zero)
			{
				vm = VectorMath.ExtractDotVector(momentum, transform.up);
				hm = momentum - vm;
			}

			//Add gravity to vertical momentum;
			vm -= transform.up * gravity * deltaTime;

			//Remove any downward force if the controller is grounded;
			if(currentControllerState == ControllerState.Grounded && VectorMath.GetDotProduct(vm, transform.up) < 0f)
				vm = Vector3.zero;

			//Apply friction to horizontal momentum based on whether the controller is grounded;
			if(currentControllerState == ControllerState.Grounded)
				hm = VectorMath.IncrementVectorTowardTargetVector(hm, groundFriction, deltaTime, Vector3.zero);
			else
				hm = VectorMath.IncrementVectorTowardTargetVector(hm, airFriction, deltaTime, Vector3.zero); 

			//Add horizontal and vertical momentum back together;
			momentum = hm + vm;
			
			if(useLocalMomentum)
				momentum = transform.worldToLocalMatrix * momentum;
		}
        
		//This function is called when the controller has lost ground contact, i.e. is either falling or rising, or generally in the air;
		void OnGroundContactLost()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if(useLocalMomentum)
				momentum = transform.localToWorldMatrix * momentum;

			//Get current movement velocity;
			Vector3 v = Vector3.zero;

			//Check if the controller has both momentum and a current movement velocity;
			if(v.sqrMagnitude >= 0f && momentum.sqrMagnitude > 0f)
			{
				//Project momentum onto movement direction;
				Vector3 projectedMomentum = Vector3.Project(momentum, v.normalized);
				//Calculate dot product to determine whether momentum and movement are aligned;
				float dot = VectorMath.GetDotProduct(projectedMomentum.normalized, v.normalized);

				//If current momentum is already pointing in the same direction as movement velocity,
				//Don't add further momentum (or limit movement velocity) to prevent unwanted speed accumulation;
				if(projectedMomentum.sqrMagnitude >= v.sqrMagnitude && dot > 0f)
					v = Vector3.zero;
				else if(dot > 0f)
					v -= projectedMomentum;	
			}

			//Add movement velocity to momentum;
			momentum += v;

			if(useLocalMomentum)
				momentum = transform.worldToLocalMatrix * momentum;
		}
		
		//Returns 'true' if controller is grounded (or sliding down a slope);
		public bool IsGrounded()
		{
			return currentControllerState == ControllerState.Grounded;
		}
		
		
		/// <summary>
		/// 强制朝向
		/// </summary>
		/// <param name="target"></param>
		public void ForceLookAt(Vector3 target)
		{
			if(!canTurn) return;
			Vector3 dir = target - unit.Position;
			dir.y = 0;
			unit.Rotation = Quaternion.LookRotation(dir, Vector3.up);
		}
    }
}
