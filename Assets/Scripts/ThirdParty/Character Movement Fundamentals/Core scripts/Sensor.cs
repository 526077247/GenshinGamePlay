using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CMF
{
	//This script is responsible for casting rays and spherecasts;
	//It is instantiated by the 'Mover' component at runtime;
	[System.Serializable]
	public class Sensor {

		//Basic raycast parameters;
		public float castLength = 1f;
		public float sphereCastRadius = 0.2f;

		//Starting point of (ray-)cast;
		private Vector3 origin = Vector3.zero;

		//Enum describing local transform axes used as directions for raycasting;
		public enum CastDirection
		{
			Forward,
			Right,
			Up,
			Backward, 
			Left,
			Down
		}

		private CastDirection castDirection;

		//Raycast hit information variables;
		private bool hasDetectedHit;
		private Vector3 hitPosition;
		private Vector3 hitNormal;
		private float hitDistance;
		private List<Collider> hitColliders = new List<Collider>();
		private List<Transform> hitTransforms = new List<Transform>();

		//Backup normal used for specific edge cases when using spherecasts;
		private Vector3 backupNormal;

		//References to attached components;
		private Transform tr;
		private Collider col;

		//Enum describing different types of ground detection methods;
		[SerializeField]
		public enum CastType
		{
			Raycast,
			RaycastArray,
			Spherecast
		}

		public CastType castType = CastType.Raycast;
		public LayerMask layermask = 255;

		//Layer number for 'Ignore Raycast' layer;
		int ignoreRaycastLayer;

		//Spherecast settings;

		//Cast an additional ray to get the true surface normal;
		public bool calculateRealSurfaceNormal = false;
		//Cast an additional ray to get the true distance to the ground;
		public bool calculateRealDistance = false;

		//Array raycast settings;

		//Number of rays in every row;
		public int arrayRayCount = 9;
		//Number of rows around the central ray;
		public int ArrayRows = 3;
		//Whether or not to offset every other row;
		public bool offsetArrayRows = false;

		//Array containing all array raycast start positions (in local coordinates);
		private Vector3[] raycastArrayStartPositions;

		//Optional list of colliders to ignore when raycasting;
		private Collider[] ignoreList;

		//Array to store layers of colliders in ignore list;
		private int[] ignoreListLayers;

		//Whether to draw debug information (hit positions, hit normals...) in the editor;
		public bool isInDebugMode = false;

		List<Vector3> arrayNormals = new List<Vector3>();
		List<Vector3> arrayPoints = new List<Vector3>();

		//Constructor;
		public Sensor (Transform _transform, Collider _collider)
		{
			tr = _transform;

			if(_collider == null)
				return;

			ignoreList = new Collider[1];

			//Add collider to ignore list;
			ignoreList[0] = _collider;

			//Store "Ignore Raycast" layer number for later;
			ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");

			//Setup array to store ignore list layers;
			ignoreListLayers = new int[ignoreList.Length];
		}

		//Reset all variables related to storing information on raycast hits;
		private void ResetFlags()
		{
			hasDetectedHit = false;
			hitPosition = Vector3.zero;
			hitNormal = -GetCastDirection();
			hitDistance = 0f;

			if(hitColliders.Count > 0)
				hitColliders.Clear();
			if(hitTransforms.Count > 0)
				hitTransforms.Clear();
		}

		//Returns an array containing the starting positions of all array rays (in local coordinates) based on the input arguments;
		public static Vector3[] GetRaycastStartPositions(int sensorRows, int sensorRayCount, bool offsetRows, float sensorRadius)
		{
			//Initialize list used to store the positions;
			List<Vector3> _positions = new List<Vector3>();

			//Add central start position to the list;
			Vector3 _startPosition = Vector3.zero;
			_positions.Add(_startPosition);

			for(int i = 0; i < sensorRows; i++)
			{
				//Calculate radius for all positions on this row;
				float _rowRadius = (float)(i+1)/sensorRows; 

				for(int j = 0; j < sensorRayCount * (i + 1); j++)
				{
					//Calculate angle (in degrees) for this individual position;
					float _angle = (360f/(sensorRayCount * (i + 1))) * j;	

					//If 'offsetRows' is set to 'true', every other row is offset;
					if(offsetRows && i % 2 == 0)	
						_angle += (360f/(sensorRayCount * (i + 1)))/2f;

					//Combine radius and angle into one position and add it to the list;
					float _x = _rowRadius * Mathf.Cos(Mathf.Deg2Rad * _angle);
					float _y = _rowRadius * Mathf.Sin(Mathf.Deg2Rad * _angle);

					_positions.Add(new Vector3(_x, 0f, _y) * sensorRadius);
				}
			}
			//Convert list to array and return array;
			return _positions.ToArray();
		}

		//Cast a ray (or sphere or array of rays) to check for colliders;
		public void Cast()
		{
			ResetFlags();

			//Calculate origin and direction of ray in world coordinates;
			Vector3 _worldDirection = GetCastDirection();
			Vector3 _worldOrigin = tr.TransformPoint(origin);

			//Check if ignore list length has been changed since last frame;
			if(ignoreListLayers.Length != ignoreList.Length)
			{
				//If so, setup ignore layer array to fit new length;
				ignoreListLayers = new int[ignoreList.Length]; 
			}

			//(Temporarily) move all objects in ignore list to 'Ignore Raycast' layer;
			for(int i = 0; i < ignoreList.Length; i++)
			{
				ignoreListLayers[i] = ignoreList[i].gameObject.layer;
				ignoreList[i].gameObject.layer = ignoreRaycastLayer;
			}

			//Depending on the chosen mode of detection, call different functions to check for colliders;
			switch (castType)
			{
				case CastType.Raycast:
					CastRay(_worldOrigin, _worldDirection);
					break;
				case CastType.Spherecast:
					CastSphere(_worldOrigin, _worldDirection);
					break;
					case CastType.RaycastArray:
					CastRayArray(_worldOrigin, _worldDirection);
					break;
				default:
					hasDetectedHit = false;
					break;
			}

			//Reset collider layers in ignoreList;
			for(int i = 0; i < ignoreList.Length; i++)
			{
				ignoreList[i].gameObject.layer = ignoreListLayers[i];
			}
		}

		//Cast an array of rays into '_direction' and centered around '_origin';
		private void CastRayArray(Vector3 _origin, Vector3 _direction)
		{
			//Calculate origin and direction of ray in world coordinates;
			Vector3 _rayStartPosition = Vector3.zero;
			Vector3 rayDirection = GetCastDirection();

			//Clear results from last frame;
			arrayNormals.Clear();
			arrayPoints.Clear();

			RaycastHit _hit;

			//Cast array;
			for(int i = 0; i < raycastArrayStartPositions.Length; i++)
			{
				//Calculate ray start position;
				_rayStartPosition = _origin + tr.TransformDirection(raycastArrayStartPositions[i]);

				if(Physics.Raycast(_rayStartPosition, rayDirection, out _hit, castLength, layermask, QueryTriggerInteraction.Ignore))
				{
					if(isInDebugMode)
						Debug.DrawRay(_hit.point, _hit.normal, Color.red, Time.fixedDeltaTime * 1.01f);

					hitColliders.Add(_hit.collider);
					hitTransforms.Add(_hit.transform);
					arrayNormals.Add(_hit.normal);
					arrayPoints.Add(_hit.point);
				}
			}

			//Evaluate results;
			hasDetectedHit = (arrayPoints.Count > 0);

			if(hasDetectedHit)
			{
				//Calculate average surface normal;
				Vector3 _averageNormal = Vector3.zero;
				for(int i = 0; i < arrayNormals.Count; i++)
				{
					_averageNormal += arrayNormals[i];
				}

				_averageNormal.Normalize();

				//Calculate average surface point;
				Vector3 _averagePoint = Vector3.zero;
				for(int i = 0; i < arrayPoints.Count; i++)
				{
					_averagePoint += arrayPoints[i];
				}

				_averagePoint /= arrayPoints.Count;
				
				hitPosition = _averagePoint;
				hitNormal = _averageNormal;
				hitDistance = VectorMath.ExtractDotVector(_origin - hitPosition, _direction).magnitude;
			}
		}

		//Cast a single ray into '_direction' from '_origin';
		private void CastRay(Vector3 _origin, Vector3 _direction)
		{
			RaycastHit _hit;
			hasDetectedHit = Physics.Raycast(_origin, _direction, out _hit, castLength, layermask, QueryTriggerInteraction.Ignore);

			if(hasDetectedHit)
			{
				hitPosition = _hit.point;
				hitNormal = _hit.normal;

				hitColliders.Add(_hit.collider);
				hitTransforms.Add(_hit.transform);

				hitDistance = _hit.distance;
			}
		}

		//Cast a sphere into '_direction' from '_origin';
		private void CastSphere(Vector3 _origin, Vector3 _direction)
		{
			RaycastHit _hit;
			hasDetectedHit = Physics.SphereCast(_origin, sphereCastRadius, _direction, out _hit, castLength - sphereCastRadius, layermask, QueryTriggerInteraction.Ignore);

			if(hasDetectedHit)
			{
				hitPosition = _hit.point;
				hitNormal = _hit.normal;
				hitColliders.Add(_hit.collider);
				hitTransforms.Add(_hit.transform);

				hitDistance = _hit.distance;

				hitDistance += sphereCastRadius;

				//Calculate real distance;
				if(calculateRealDistance)
				{
					hitDistance = VectorMath.ExtractDotVector(_origin - hitPosition, _direction).magnitude;
				}

				Collider _col = hitColliders[0];

				//Calculate real surface normal by casting an additional raycast;
				if(calculateRealSurfaceNormal)
				{
					if(_col.Raycast(new Ray(hitPosition - _direction, _direction), out _hit, 1.5f))
					{
						if(Vector3.Angle(_hit.normal, -_direction) >= 89f)
							hitNormal = backupNormal;
						else
							hitNormal = _hit.normal;
					}
					else
						hitNormal = backupNormal;
					
					backupNormal = hitNormal;
				}
			}
		}

		//Calculate a direction in world coordinates based on the local axes of this gameobject's transform component;
		Vector3 GetCastDirection()
		{
			switch(castDirection)
			{
			case CastDirection.Forward:
				return tr.forward;

			case CastDirection.Right:
				return tr.right;

			case CastDirection.Up:
				return tr.up;

			case CastDirection.Backward:
				return -tr.forward;

			case CastDirection.Left:
				return -tr.right;

			case CastDirection.Down:
				return -tr.up;
			default:
				return Vector3.one;
			}
		}

		//Draw debug information in editor (hit positions and ground surface normals);
		public void DrawDebug()
		{
			if(hasDetectedHit && isInDebugMode)
			{
				Debug.DrawRay(hitPosition, hitNormal, Color.red, Time.deltaTime);
				float _markerSize = 0.2f;
				Debug.DrawLine(hitPosition + Vector3.up * _markerSize, hitPosition - Vector3.up * _markerSize, Color.green, Time.deltaTime);
				Debug.DrawLine(hitPosition + Vector3.right * _markerSize, hitPosition - Vector3.right * _markerSize, Color.green, Time.deltaTime);
				Debug.DrawLine(hitPosition + Vector3.forward * _markerSize, hitPosition - Vector3.forward * _markerSize, Color.green, Time.deltaTime);
			}
		}

		//Getters;

		//Returns whether the sensor has hit something;
		public bool HasDetectedHit()
		{
			return hasDetectedHit;
		}

		//Returns how far the raycast reached before hitting a collider;
		public float GetDistance()
		{
			return hitDistance;
		}

		//Returns the surface normal of the collider the raycast has hit;
		public Vector3 GetNormal()
		{
			return hitNormal;
		}

		//Returns the position in world coordinates where the raycast has hit a collider;
		public Vector3 GetPosition()
		{
			return hitPosition;
		}

		//Returns a reference to the collider that was hit by the raycast;
		public Collider GetCollider()
		{
			return hitColliders[0];
		}

		//Returns a reference to the transform component attached to the collider that was hit by the raycast;
		public Transform GetTransform()
		{
			return hitTransforms[0];
		}

		//Setters;

		//Set the position for the raycast to start from;
		//The input vector '_origin' is converted to local coordinates;
		public void SetCastOrigin(Vector3 _origin)
		{
			if(tr == null)
				return;
			origin = tr.InverseTransformPoint(_origin);
		}

		//Set which axis of this gameobject's transform will be used as the direction for the raycast;
		public void SetCastDirection(CastDirection _direction)
		{
			if(tr == null)
				return;

			castDirection = _direction;
		}

		//Recalculate start positions for the raycast array;
		public void RecalibrateRaycastArrayPositions()
		{
			raycastArrayStartPositions = GetRaycastStartPositions(ArrayRows, arrayRayCount, offsetArrayRows, sphereCastRadius);
		}
	}
}