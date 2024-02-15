using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankEnemy : MonoBehaviour
{
    public float m_Speed = 12f;                 // How fast the tank moves forward and back.
    public float m_TurnSpeed = 180f;            // How fast the tank turns in degrees per second.
    public AudioSource m_MovementAudio;         // Reference to the audio source used to play engine sounds. NB: different to the shooting audio source.
    public AudioClip m_EngineIdling;            // Audio to play when the tank isn't moving.
    public AudioClip m_EngineDriving;           // Audio to play when the tank is moving.
    public float m_PitchRange = 0.2f;           // The amount by which the pitch of the engine noises can vary.s
    public float m_AimingDistance = 20f;

    private Rigidbody m_Rigidbody;              // Reference used to move the tank.
    private TankShooting m_Shooting;
    private float m_MovementValue;              // The current value of the movement input.
    private float m_TurnValue;                  // The current value of the turn input.
    private float m_OriginalPitch;              // The pitch of the audio source at the start of the scene.
    private GameObject closestPlayerTank;       // Reference to the closest player tank.

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Shooting = GetComponent<TankShooting>();
    }

    private void OnEnable()
    {
        m_MovementValue = 0f;
        m_TurnValue = 0f;
    }

    private void Start()
    {
        m_OriginalPitch = m_MovementAudio.pitch;
    }

    private void Update()
    {
        closestPlayerTank = FindClosestPlayerTank();
        if (closestPlayerTank != null)
        {
            CalculateMovement();
            EngineAudio();
        }
    }

    private GameObject FindClosestPlayerTank()
    {
        GameObject[] playerTanks = GameObject.FindGameObjectsWithTag("PlayerTank");
        GameObject closestTank = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject tank in playerTanks)
        {
            Vector3 directionToTarget = tank.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closestTank = tank;
            }
        }

        return closestTank;
    }

    private void FixedUpdate()
    {
        CalculateMovement();
        Move();
        Turn();
    }

    private void CalculateMovement()
    {
        if (closestPlayerTank != null)
        {
            // Calculate direction towards the closest player tank
            Vector3 direction = closestPlayerTank.transform.position - transform.position;
            direction.y = 0f; // Ensure the direction is only in the XZ plane

            // Calculate the angle between the tank's forward vector and the direction to the target
            float angle = Vector3.Angle(transform.forward, direction);

            float sign = Mathf.Sign(Vector3.Dot(transform.right, direction));

            // Set turn value based on the sign of the angle
            m_TurnValue = sign * angle / 180f;

            // Set movement value to move forward

            // Calculate the distance to the target
            float distanceToTarget = direction.magnitude;

            if (distanceToTarget > m_AimingDistance)
            {
                m_Shooting.m_IsAiming = false;
                // Move towards the target
                m_MovementValue = 1f;
            }
            else
            {
                m_Shooting.m_IsAiming = true;
                m_MovementValue = 0f;
            }
        }
        else
        {
            m_MovementValue = 0f;
            m_TurnValue = 0f;
        }
    }

    private void Move()
    {
        // Create a vector in the direction the tank is facing with a magnitude based on the movement value, speed, and the time between frames.
        Vector3 movement = transform.forward * m_MovementValue * m_Speed * Time.deltaTime;

        // Apply this movement to the rigidbody's position.
        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
    }

    private void Turn()
    {
        // Determine the number of degrees to be turned based on the turn value, speed, and time between frames.
        float turn = m_TurnValue * m_TurnSpeed * Time.deltaTime;

        // Make this into a rotation in the y-axis.
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

        // Apply this rotation to the rigidbody's rotation.
        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
    }

    private void EngineAudio()
    {
        if (Mathf.Approximately(m_MovementValue, 0f) && Mathf.Approximately(m_TurnValue, 0f))
        {
            if (m_MovementAudio.clip == m_EngineDriving)
            {
                m_MovementAudio.clip = m_EngineIdling;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
        else
        {
            if (m_MovementAudio.clip == m_EngineIdling)
            {
                m_MovementAudio.clip = m_EngineDriving;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
    }
}