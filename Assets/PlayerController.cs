using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 200f;
    [SerializeField] private bool rawInput = false;
    [SerializeField] private Camera cam;
    [SerializeField] private float crouchSpeed = 150f;
    [SerializeField] private float walkSpeed = 300f;
    [SerializeField] private float sprintSpeed = 600f;
    [SerializeField] private float headBobAmount = 0.05f;
    [SerializeField] private float headBobSpeed = 14f;
    [SerializeField] private LayerMask pickableLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float pickableDistance = 1.5f;
    [SerializeField] private Transform gunPos;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float gravity = 10f;
    [SerializeField] private Transform feet;
    [SerializeField] private float feetRadius = 0.5f;
    [SerializeField] private float adsSpeed = 0.15f;
    [SerializeField] private float adsFOV = 30;
    [SerializeField] private float fov = 60;

    private Rigidbody rb;
    private float rotX;
    private float rotY;
    private float bobTimer;
    private GameObject pickItem = null;
    private PhotonView view;
    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        view = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Camera
        if (view.IsMine)
            cam.enabled = true;
        else
            Destroy(this);
    }

    void Update()
    {
        if (view.IsMine == false)
            return;

        bool isGrounded = Physics.CheckSphere(feet.position, feetRadius, groundLayer);
            
        // Look
        float mX, mY;
        
        if (rawInput)
        {
            mX = Input.GetAxisRaw("Mouse X");
            mY = Input.GetAxisRaw("Mouse Y");
        }
        else
        {
            mX = Input.GetAxis("Mouse X");
            mY = Input.GetAxis("Mouse Y");
        }

        rotX += mX * mouseSensitivity * Time.deltaTime;
        rotY += mY * mouseSensitivity * Time.deltaTime;
 
        rotY = Mathf.Clamp(rotY, -90f, 90f);      
 
        cam.transform.localRotation = Quaternion.Euler(-rotY, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, rotX, 0f);

        // Move
        float x = Input.GetAxis("Horizontal"), y = Input.GetAxis("Vertical");
        Vector3 move = (transform.forward * y + transform.right * x).normalized;
        
        if (Input.GetKey(KeyCode.LeftControl))
            controller.Move(move * Time.deltaTime * crouchSpeed);
        if (Input.GetKey(KeyCode.LeftShift))
            controller.Move(move * Time.deltaTime * sprintSpeed);
        else
            controller.Move(move * Time.deltaTime * walkSpeed);

        if (isGrounded && velocity.y <= 0f)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Crouching
        if (Input.GetKey(KeyCode.LeftControl))
        {
            cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, Mathf.Lerp(cam.transform.localPosition.y, 0f, Time.deltaTime * headBobSpeed), cam.transform.localPosition.z);
        }

        // Head bobbing
        if (Input.GetKey(KeyCode.LeftControl) == false)
        {
            if(move.magnitude > 0.1f)
            {
                bobTimer += Time.deltaTime * headBobSpeed;
                cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, 0.5f + Mathf.Sin(bobTimer) * headBobAmount, cam.transform.localPosition.z);
            }
            else
            {
                bobTimer = 0;
                cam.transform.localPosition = new Vector3(cam.transform.localPosition.x, Mathf.Lerp(cam.transform.localPosition.y, 0.5f, Time.deltaTime * headBobSpeed), cam.transform.localPosition.z);
            }
        }

        // Pickup
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Physics.Raycast(ray, out hit, pickableDistance, pickableLayer))
            {
                ThrowItem();

                pickItem = hit.transform.gameObject;

                pickItem.transform.parent = transform;
                pickItem.transform.localPosition = gunPos.localPosition;
                pickItem.transform.rotation = gunPos.rotation;
                pickItem.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
            }
        }

        if (pickItem != null)
        {
            pickItem.transform.position = Vector3.Lerp(pickItem.transform.position, gunPos.position, 0.15f);
            pickItem.transform.rotation = Quaternion.Lerp(pickItem.transform.rotation, gunPos.rotation, 0.15f);
        }

        // Throw
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ThrowItem(throwForce);
        }

        // ADS
        if (Input.GetMouseButton(1))
        {
            gunPos.transform.localPosition = new Vector3(Mathf.Lerp(gunPos.transform.localPosition.x, 0f, adsSpeed), gunPos.transform.localPosition.y, gunPos.transform.localPosition.z);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, adsFOV, adsSpeed);
        }
        else
        {
            gunPos.transform.localPosition = new Vector3(Mathf.Lerp(gunPos.transform.localPosition.x, 0.5f, adsSpeed), gunPos.transform.localPosition.y, gunPos.transform.localPosition.z);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, adsSpeed);
        }
    }

    void ThrowItem(float force = 0f)
    {
        if (pickItem != null)
        {
            pickItem.GetComponent<Item>().Drop(pickItem.transform);
            pickItem.transform.parent = null;
            pickItem = null;
        }
    }
}
