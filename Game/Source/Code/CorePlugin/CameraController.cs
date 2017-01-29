using System;
using System.Collections.Generic;
using System.Linq;

using Duality;
using Duality.Components;
using Duality.Editor;

namespace Game
{
	public class CameraController : Component, ICmpUpdatable
	{
		private float           smoothness     = 1.0f;
		private Camera          camera         = null;
		private SoundListener   microphone     = null;
		private List<Transform> followTargets  = new List<Transform>();
		private float           zoomOutScale   = 1.0f;
		private float           maxZoomOutDist = 350.0f;

		[DontSerialize] private Vector3 focusPos          = Vector3.Zero;
		[DontSerialize] private float   focusRadius       = 0.0f;
		[DontSerialize] private float   screenShake       = 0.0f;
		[DontSerialize] private Vector3 screenShakeOffset = Vector3.Zero;
		[DontSerialize] private float   screenShakeAngle  = 0.0f;


		[EditorHintRange(0.0f, 10.0f)]
		public float Smoothness
		{
			get { return this.smoothness; }
			set { this.smoothness = value; }
		}
		public float MaxZoomOutDist
		{
			get { return this.maxZoomOutDist; }
			set { this.maxZoomOutDist = value; }
		}
		public float ZoomOutScale
		{
			get { return this.zoomOutScale; }
			set { this.zoomOutScale = value; }
		}
		public Camera Camera
		{
			get { return this.camera; }
			set { this.camera = value; }
		}
		public SoundListener Microphone
		{
			get { return this.microphone; }
			set { this.microphone = value; }
		}
		public List<Transform> FollowTargets
		{
			get { return this.followTargets; }
			set { this.followTargets = value ?? new List<Transform>(); }
		}


		public void ShakeScreen(float strength, Vector3 hitPosition)
		{
			float distanceFromFocus = (hitPosition - this.focusPos).Length / 350.0f;
			float distanceFactor = 1.0f / (1.0f + distanceFromFocus);
			this.screenShake += distanceFactor * strength;
		}
		
		private Vector3 GetTargetOffset(float viewRadius)
		{
			float zoomThreshold = 200.0f;
			float zoomOutDistance = this.zoomOutScale * MathF.Max(0, viewRadius - zoomThreshold);
			zoomOutDistance = MathF.Min(this.maxZoomOutDist, zoomOutDistance);
			return -new Vector3(0.0f, 0.0f, this.camera.FocusDist + zoomOutDistance);
		}

		void ICmpUpdatable.OnUpdate()
		{
			if (this.camera == null) return;
			if (this.microphone == null) return;

			Transform camTransform = this.camera.GameObj.Transform;
			Transform microTransform = this.microphone.GameObj.Transform;

			// Update screen shake behavior
			Vector3 lastScreenShakeOffset = this.screenShakeOffset;
			float lastScreenShakeAngle = this.screenShakeAngle;
			this.screenShakeOffset = MathF.Rnd.NextVector3() * 100.0f * this.screenShake;
			this.screenShakeAngle = MathF.Rnd.NextFloat(-1.0f, 1.0f) * MathF.DegToRad(5.0f) * this.screenShake;
			this.screenShake += (0.0f - this.screenShake) * 0.2f * Time.TimeMult;

			// Remove old screen shake 
			camTransform.Pos -= lastScreenShakeOffset;
			camTransform.Angle -= lastScreenShakeAngle;
			
			// Remove disposed / null follow targets
			this.followTargets.RemoveAll(obj => obj == null || obj.Disposed);

			// Follow a group of objects behavior
			if (this.followTargets.Count > 0)
			{
				// Determine the position to focus on. It's the average of all follow object positions.
				this.focusPos = Vector3.Zero;
				foreach (Transform obj in this.followTargets)
				{
					this.focusPos += obj.Pos;
				}
				this.focusPos /= this.followTargets.Count;

				// Determine how far these objects are away from each other
				this.focusRadius = 0.0f;
				foreach (Transform obj in this.followTargets)
				{
					this.focusRadius = MathF.Max((obj.Pos - this.focusPos).Length, this.focusRadius);
				}

				// Move the camera so it can most likely see all of the required objects
				Vector3 targetPos = this.focusPos + this.GetTargetOffset(this.focusRadius);
				Vector3 posDiff = (targetPos - camTransform.Pos);
				Vector3 targetVelocity = posDiff * 0.1f * MathF.Pow(2.0f, -this.smoothness);
				camTransform.MoveByAbs(targetVelocity * Time.TimeMult);
				microTransform.MoveToAbs(new Vector3(camTransform.Pos.Xy));
			}

			// Apply new screen shake
			camTransform.Pos += this.screenShakeOffset;
			camTransform.Angle += this.screenShakeAngle;
		}
	}
}
