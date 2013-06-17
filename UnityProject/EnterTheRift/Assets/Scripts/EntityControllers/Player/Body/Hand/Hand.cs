using System.Collections.Generic;
using UnityEngine;
using System;

public class Hand : MonoBehaviour 
{
	private enum ControllerId
	{
		Undefined = -1,
		Left = 0,
		Right = 1
	}
	
	[SerializeField] private ControllerId controllerId = ControllerId.Undefined;
	[SerializeField] private Animation handAnimation = null;
	[SerializeField] private HandGrab dynamicCollider = null;
	[SerializeField] private Transform cameraMount = null;
	
	[SerializeField] private Vector3 offset;
	[SerializeField] private Vector2 leftStick;
	[SerializeField] private Vector2 rightStick;
	[SerializeField] private Vector3 handWorldPosition;
	
	public bool IsFistDisabled { get; set; }
	
	
	#region MonoBehaviour
	
	void Awake ()
	{
		Initialize();
	}
	
	void Start ()
	{
		if (ManagerFactory.InputManager.InputId != InputId.Hydra)
		{
			this.offset = new Vector3(0.0f, 0.0f, 1.0f);
		}
	}
	
	#endregion MonoBehaviour
	
	
	#region Initialization
	
	private void Initialize ()
	{
		this.IsFistDisabled = true;
	}
	
	#endregion Initialization
	
	
	#region Input Control Callbacks
	
	private void OnHydraTrigger (float input)
	{
		if (input < InputManager.HydraSensitivity.TriggerPress)
		{
			// Trigger is not pressed.
			
			if (input < InputManager.HydraSensitivity.TriggerRelease) 
			{
				SetHandFist(false);
			}
			return;
		}
		
		SetHandFist(true);
	}
	
	private void OnHydraStick (Vector2 input)
	{
		
	}
	
	private void OnHydraPosition (Vector3 input)
	{
		
	}
	
	private void OnHydraRotation (Quaternion input)
	{
		
	}
	
	#endregion Input Control Callbacks
	
	
	#region Hydra Input Controls
	
	private void UpdateHydra ()
	{
		SixenseInput.Controller controller = SixenseInput.Controllers[(int) this.controllerId];
		
		// Trigger data.
		UpdateHydraTrigger(controller);
		
		// Position data.
		UpdateHydraPosition(controller);
			
		// Rotation data.
		UpdateHydraRotation(controller);
		
		// Send analog stick values to the player, for movement purposes.
		UpdateHydraAnalogStick(controller);
	}
	
	private void UpdateHydraTrigger (SixenseInput.Controller controller)
	{
		InputManager inputManager = ManagerFactory.InputManager;
		
		if (controller.Trigger < InputManager.HydraSensitivity.TriggerPress)
		{
			// Trigger is not pressed.
			
			if (controller.Trigger < InputManager.HydraSensitivity.TriggerRelease) 
			{
				SetHandFist(false);
			}
			
			return;
		}
		
		if (inputManager.CanCalibrate
			&& !inputManager.IsCalibrated)
		{
			CalibrateHydra(controller);
			return;
		}
		
		SetHandFist(true);
	}
	
	private void CalibrateHydra (SixenseInput.Controller controller)
	{
		this.offset = new Vector3(0.0f, 0.0f, controller.Position.z * InputManager.HydraSensitivity.Position);
		ManagerFactory.InputManager.IsCalibrated = true;
	}
	
	private void UpdateHydraPosition (SixenseInput.Controller controller)
	{
		Vector3 controllerPosition = controller.Position;
		Vector3 desiredLocalPosition = new Vector3(
			controllerPosition.x * InputManager.HydraSensitivity.Position,
			controllerPosition.y * InputManager.HydraSensitivity.Position,
			controllerPosition.z * InputManager.HydraSensitivity.Position) 
			- this.offset;
		
		if (this.cameraMount != null)
		{
			desiredLocalPosition = cameraMount.transform.localRotation * desiredLocalPosition;
		}
		
		if (this.IsFistDisabled)
		{
			if (controllerId == ControllerId.Left)
			{
				desiredLocalPosition = new Vector3(-1.0f, -0.5f, 1.0f);
			}
			if (controllerId == ControllerId.Right)
			{
				desiredLocalPosition = new Vector3(1.0f, -0.5f, 1.0f);
			}
		}
		
		this.transform.localPosition = desiredLocalPosition;
	}
	
	private void UpdateHydraRotation (SixenseInput.Controller controller)
	{
		transform.localRotation = new Quaternion(controller.Rotation.x,
												 controller.Rotation.y,
												 controller.Rotation.z,
												 controller.Rotation.w);
		if (this.IsFistDisabled)
		{
			this.transform.localRotation = new Quaternion();
		}
	}
	
	private void UpdateHydraAnalogStick (SixenseInput.Controller controller)
	{
		if (controllerId == ControllerId.Left) 
		{
			this.leftStick = new Vector2(controller.JoystickX, controller.JoystickY);
		}
		else if (controllerId == ControllerId.Right) 
		{
			this.rightStick = new Vector2(controller.JoystickX, controller.JoystickY);
		}
	}
	
	#endregion Hydra Input Controls
	
	
	#region Gamepad Input Controls
	
	/*
	private void UpdateGamepad ()
	{
		float triggerValue = Input.GetAxis ("Triggers");

		if (controllerId == 0)
		{
			if (triggerValue > 0.8f)
			{
				leftStick = new Vector2();
				transform.localPosition = new Vector3(
					(Mathf.Clamp (Input.GetAxis ("HorizontalL"), -0.75f, 0.75f) - 0.25f) * 2.0f,
					Mathf.Clamp (Input.GetAxis ("VerticalR"), -0.75f, 0.75f) * -2.0f,
					Mathf.Clamp (Input.GetAxis ("VerticalL"), -0.75f, 0.75f) + 0.25f) +
					offset;
				
				if (!isTriggerDown 
					&& handAnimation != null)
				{
					handAnimation.Play("fist");
					isTriggerDown = true;
				}
			}
			else
			{
				leftStick = new Vector2(Input.GetAxis ("HorizontalL"), Input.GetAxis ("VerticalL"));
				
				if(isTriggerDown && handAnimation != null)
				{
					handAnimation.Play("unfist");
					isTriggerDown = false;
				}
			}
		}
		else if (this.controllerId == ControllerId.Right)
		{
			if(triggerValue < -0.8f)
			{
				rightStick = new Vector2();
				transform.localPosition = new Vector3(
											(Mathf.Clamp (Input.GetAxis ("HorizontalL"), -0.75f, 0.75f) + 0.25f) * 2.0f,
											Mathf.Clamp (Input.GetAxis ("VerticalR"), -0.75f, 0.75f) * -2.0f,
											Mathf.Clamp (Input.GetAxis ("VerticalL"), -0.75f, 0.75f) + 0.25f) +
										  offset;
				
				if(!isTriggerDown && handAnimation != null)
				{
					handAnimation.Play("fist");
					isTriggerDown = true;
				}
			}
			else
			{
				rightStick = new Vector2(Input.GetAxis ("HorizontalR"), Input.GetAxis("VerticalR"));
				
				if(isTriggerDown && handAnimation != null)
				{
					handAnimation.Play("unfist");
					isTriggerDown = false;
				}
			}
		}
	}
	*/
	
	#endregion Gamepad Input Controls
	

	#region Hand Control
	
	public void SetHandFist (bool isEnabled)
	{
		bool isHandAnimationSet = this.handAnimation != null;
		
		if (isEnabled)
		{
			// Fist
			this.dynamicCollider.GrabItems();
			
			if (isHandAnimationSet)
			{
				this.handAnimation.Play("fist");
			}
		} 
		else 
		{
			// Unfist
			this.dynamicCollider.LetGoOfItems();
			
			if (isHandAnimationSet)
			{
				this.handAnimation.Play ("unfist");
			}
		}
		
	}
	#endregion Hand Control
}
