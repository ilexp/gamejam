using System;
using System.Collections.Generic;
using System.Linq;

using Duality;
using Duality.Components;

namespace Game
{
	/// <summary>
	/// Camera controller that supports following a target on a smooth curve and screen shake.
	/// For screen shake to work, the <see cref="Camera"/> needs to be on a child object of the
	/// controller, rather than the controller object itself.
	/// </summary>
	[RequiredComponent(typeof(Transform))]
	public class CameraController : Component, ICmpUpdatable
	{
		private float smoothness = 1.0f;
		private GameObject targetObj = null;
		private Transform cameraOffsetTransform = null;
		private float screenShake = 0.0f;

		public float Smoothness
		{
			get { return this.smoothness; }
			set { this.smoothness = value; }
		}
		public GameObject TargetObject
		{
			get { return this.targetObj; }
			set { this.targetObj = value; }
		}
		/// <summary>
		/// [GET / SET] The <see cref="Transform"/> to which the actual <see cref="Camera"/> object is parented.
		/// Should be a direct child of this object.
		/// </summary>
		public Transform CameraOffsetTransform
		{
			get { return this.cameraOffsetTransform; }
			set { this.cameraOffsetTransform = value; }
		}

		public void ShakeScreen(float strength)
		{
			this.screenShake += strength;
		}

		void ICmpUpdatable.OnUpdate()
		{
			if (this.targetObj == null) return;
			if (this.targetObj.Transform == null) return;

			Transform transform = this.GameObj.Transform;
			Transform offsetTransform = this.cameraOffsetTransform;
			Camera camera = (offsetTransform ?? transform).GameObj.GetComponent<Camera>();
			if (camera == null) return;

			Vector3 focusPos = this.targetObj.Transform.Pos;
			Vector3 targetPos = focusPos - new Vector3(0.0f, 0.0f, camera.FocusDist);
			Vector3 posDiff = (targetPos - transform.Pos);
			Vector3 targetVelocity = posDiff * 0.1f * MathF.Pow(2.0f, -this.smoothness);

			transform.MoveByAbs(targetVelocity * Time.TimeMult);

			this.screenShake += (0.0f - this.screenShake) * 0.2f * Time.TimeMult;
			if (offsetTransform != null)
			{
				offsetTransform.MoveTo(MathF.Rnd.NextVector3() * 100.0f * this.screenShake);
				offsetTransform.TurnTo(MathF.Rnd.NextFloat(-1.0f, 1.0f) * MathF.DegToRad(5.0f) * this.screenShake); 
			}
		}
	}
}
