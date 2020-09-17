using UnityEngine;
using Mirror;
using System.Collections;
using Mirror.Examples.RigidbodyPhysics;

/*
	Documentation: https://mirror-networking.com/docs/Guides/NetworkBehaviour.html
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

public class Player : NetworkBehaviour
{
    // COMPONENTS
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private Animator anim;
    [HideInInspector] public CameraRotator playerCamera = null;

    [Header("Movement")]
    public bool startControllable = false;
    [SerializeField] private LayerMask walkableSurface;
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private float maxSpeed = 10.0f;
    [SerializeField] private float counterMoveScale = 0.1f;
    [SerializeField] private float slopeThreshold = 0.2f;
    [SerializeField] private float gravForceMultiplier = 20.0f;
    [SerializeField] private float collisionForceThreshold = 50.0f;
    private float rotationAngle = 0.0f;

    private Vector3 inputDir = Vector3.zero;
    private bool isGrounded = false;
    private bool playerStable = true;
    private bool playerControl = true;
    private bool hasFallenOver = false;
    private bool smacked = false;

    [Header("Ability Values")]
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float jumpCheckDist = 0.25f;
    [SerializeField] private float diveWaitTime = 1.0f;
    [SerializeField] private Vector3 diveForce = Vector3.forward;
    [SerializeField] private float diveTorque = 90.0f;
    [SerializeField, Min(0.1f)] private float diveRecoveryMaxSpeed = 2.0f;
    [SerializeField] private float stableHitThreshold = 5.0f;

    #region Mirror Methods

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer)
        {
            enabled = false;
        }
    }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer() 
    {
        FindObjectOfType<NetworkManagerHUD>().showGUI = false;

        if (!startControllable)
            this.enabled = false;

        playerCamera = Camera.main.GetComponent<CameraRotator>();

        // Tell Server we're waiting to play
        if (GameManager.singleton)
            GameManager.singleton.CmdReadyPlayer(GetComponent<NetworkIdentity>());
        else
            playerCamera.cameraTarget = transform;
    }
    #endregion

    // UPDATE + FIXEDUPDATE aren't exactly 'Server Authorative'... but it's been hours, I can't be arsed and you shouldn't be able to read this
    [ClientCallback]
    private void Update()
    {
        if (!isLocalPlayer)
            return;

        inputDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;

        // If can move...
        if (playerControl)
        {
            // DIVE
            if(Input.GetKeyDown(KeyCode.Mouse0))
            {
                playerControl = false;
                playerStable = false;
                anim.SetBool("Stable", playerStable);
                anim.SetTrigger("Dive");

                rb.velocity = Vector3.zero;
                rb.AddForce(transform.rotation * diveForce, ForceMode.Impulse);
                rb.AddRelativeTorque(new Vector3(diveTorque, 0,0), ForceMode.Impulse);
            }

            // JUMP
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                anim.SetTrigger("Jump");
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }

        anim.SetFloat("Movement", inputDir.magnitude);
        anim.SetBool("Grounded", isGrounded);
    }

    [ClientCallback]
    private void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;

        // Gravity Multiplier - increased vertical accuracy
        rb.AddForce(Vector3.down * Time.deltaTime * 10);
        isGrounded = GroundCheck();

        if (playerControl)
        {
            Move();
        }

        // Constrain rotation when stable
        if (playerStable)
            rb.MoveRotation(Quaternion.Euler(0, rotationAngle, 0));
    }

    /// <summary>
    /// Handles Movement
    /// </summary>
    private void Move()
    {
        if (inputDir.magnitude > 0)
        {
            // Rotation
            rotationAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + playerCamera.transform.eulerAngles.y;

            // Movement
            if (isGrounded && !hasFallenOver)
            {
                if (rb.velocity.magnitude < maxSpeed)
                    rb.AddForce(transform.forward * inputDir.magnitude * speed);
            }
        }
        else if (isGrounded)
        {
            // Counter Movement & Sliding Prevention (not particularly great method)
            rb.AddForce(speed * Vector3.forward * Time.deltaTime * -rb.velocity.z * counterMoveScale);
            rb.AddForce(speed * Vector3.right * Time.deltaTime * -rb.velocity.x * counterMoveScale);
        }
    }

    /// <summary>
    /// Returns whether player is stood on Ground/Walkable Surface
    /// </summary>
    private bool GroundCheck()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit, jumpCheckDist, walkableSurface);

        if (hit.transform)
            return true;
        else
            return false;
    }

    [ClientCallback]
    private void OnCollisionEnter(Collision collision)
    {
        if(!smacked)
        {
            float collisionForce = collision.impulse.magnitude;
            if (collisionForce > collisionForceThreshold)
            {
                playerStable = false;
                playerControl = false;
                rb.AddForce(collision.impulse * 0.25f, ForceMode.Impulse);
            }
        }

    }

    [ClientCallback]
    private void OnCollisionStay(Collision collision)
    {
        if (!hasFallenOver && !collision.transform.CompareTag("Player"))
        {
            HandleHasFallenOver();
        }
    }

    /// <summary>
    /// Handles fallen over check and resulting logic.
    /// </summary>
    private void HandleHasFallenOver()
    {
        // Fallen over Check
        if (Vector3.Dot(Vector3.down, transform.up) > -0.2f)
        {
            hasFallenOver = true;
            StartCoroutine(StandUp());
        }
    }

    /// <summary>
    /// Instantly returns Player back to standing after lying delay.
    /// </summary>
    private IEnumerator StandUp()
    {
        // Delay...
        yield return new WaitForSeconds(diveWaitTime);

        // Wait until speed while fallen is slowed
        yield return new WaitUntil(() => rb.velocity.magnitude < diveRecoveryMaxSpeed);

        // Reset pos + rot
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = transform.position + Vector3.up * 0.6f;
        rb.rotation = Quaternion.Euler(new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z));
        
        hasFallenOver = false;
        playerControl = true;
        playerStable = true;
        smacked = false;
        anim.SetBool("Stable", playerStable);
    }

    private void OnEnable()
    {
        if(playerCamera)
            playerCamera.cameraTarget = transform;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, Vector3.down * jumpCheckDist);
    }
}