using System.Collections;
using UnityEngine;


namespace NuitrackSDK.Tutorials.ZombieVR
{
    [AddComponentMenu("NuitrackSDK/Tutorials/ZombieVR/Zombie Controller")]
    public class ZombieController : MonoBehaviour
    {
        [SerializeField] int hp = 100; // health of a zombie
        [SerializeField] float damage = 0.01f; // damage from a zombie
        [SerializeField] float speed = 1; // speed of a zombie
        [SerializeField] Transform floorChecker; // used to switch Ragdoll
        [SerializeField] Animator animator; // used to control the animation of zombies
        [SerializeField] float attackDistance = 0.7f; // attacking distance of a zombie
        [SerializeField] Transform modelTransform; // used to process Ragdoll
        float standTime = 0, flyTime = 0; // time on the ground and in flight
        bool isOnGround = false; // check whether the zombie is on the ground or not
        bool isFly = false; // check whether the zombie is in flight or not
        bool isRagdoll = true; // is Ragdoll on?
        bool canAttack = false, prevCanAttack = false; // can a zombie attack?

        Player target; // target for a zombie attack (player)
        Rigidbody rb;

        Rigidbody[] rigidbodyRagdoll;
        Collider[] colliderRagdoll;

        Vector3 localPosition; // modelTransform position of a zombie

        void Awake()
        {
            localPosition = modelTransform.localPosition;
        }

        void Start()
        {
            rigidbodyRagdoll = GetComponentsInChildren<Rigidbody>();
            colliderRagdoll = GetComponentsInChildren<Collider>();
            rb = GetComponent<Rigidbody>();

            SwitchRagdoll(false); // disable ragdoll
        }

        void SwitchRagdoll(bool ragdoll)
        {
            if (ragdoll != isRagdoll)
            {
                if (ragdoll) // If ragdoll is off
                {
                    for (int i = 0; i < rigidbodyRagdoll.Length; i++)
                    {
                        rigidbodyRagdoll[i].isKinematic = false; // Physics is turned on
                        rigidbodyRagdoll[i].velocity = rb.velocity; // When ragdoll is on, the speed of the main Rigidbody component is passed to the child Rigidbody components so they continue to fly according to physics
                    }
                }
                else // If ragdoll is off
                {
                    // Return position and rotation to original state when Ragdoll is over
                    modelTransform.localRotation = Quaternion.identity;
                    transform.position = modelTransform.position; // When Ragdoll is over, the model base object is brought back to Ragdoll coordinates
                    modelTransform.localPosition = localPosition; // Move the child model to its original local coordinates

                    for (int i = 0; i < rigidbodyRagdoll.Length; i++)
                    {
                        rigidbodyRagdoll[i].isKinematic = true;
                    }
                }

                rb.isKinematic = ragdoll; // Switch the basic Ragdoll kinematics

                for (int i = 0; i < colliderRagdoll.Length; i++)
                {
                    colliderRagdoll[i].enabled = ragdoll; // Switch the Ragdoll colliders
                }

                GetComponent<Collider>().enabled = !ragdoll; // Switch the base collider

                animator.enabled = !ragdoll; // Switch the animator. When Ragdoll is turned on, еру animator is switched off.
            }

            isRagdoll = ragdoll;
        }

        bool IsOnGround() // Is the zombie on the ground?
        {
            floorChecker.rotation = Quaternion.identity; // Fix the object rotation
            Vector3 direction = -floorChecker.up; // Downward direction
            float maxDistance = 0.5f;
            Ray ray = new Ray(floorChecker.position, direction); // Create a ray

            return Physics.Raycast(ray, maxDistance); // Return value from the ray
        }

        void Update()
        {
            if (hp <= 0)
                return; // If the zombie is dead, the code below is not executed

            isOnGround = IsOnGround();

            if (target == null)
            {
                target = FindObjectOfType<Player>(); // find the target for zombies
                return;
            }

            Vector3 targetPos = new Vector3(target.transform.position.x, 0, target.transform.position.z);

            canAttack = isOnGround && Vector3.Distance(transform.position, targetPos) <= attackDistance && hp > 0; // If the zombie is on the ground, the distance to the player is sufficient and he has enough lives, it's time to attack

            if (canAttack != prevCanAttack) // Called just once
            {
                prevCanAttack = canAttack;
                StartCoroutine(Attacking());
            }

            animator.SetBool("Attacking", canAttack); // Start "attacking" animation

            if (isOnGround)
            {
                if (standTime > 2.0f) // Zombie gets up in 2 seconds
                {
                    if (isRagdoll) // If Ragdoll was off, turn it on
                        SwitchRagdoll(false);

                    transform.LookAt(targetPos); // Zombie turns to the player
                    rb.AddForce(transform.forward * speed); // And runs!
                }

                standTime += Time.deltaTime;

                if (isFly) // If the zombie has flown and fallen
                {
                    isFly = false;

                    GetDamage((int)(flyTime * 10)); // When the zombie falls, he gets damaged depending on the "flight time"

                    flyTime = 0;
                }
            }
            else
            {
                standTime = 0;
                isFly = true; // If the zombie is flying

                flyTime += Time.deltaTime;

                if (flyTime >= .3f && !isRagdoll) // If the zombie flies for more than 0.3 sec, turn the ragdoll on
                    SwitchRagdoll(true);
            }
        }

        IEnumerator Attacking()
        {
            yield return new WaitForSeconds(1.0f);

            if (hp > 0)
            {
                target.GetDamage(damage);

                if (canAttack)
                    StartCoroutine(Attacking());
            }
        }

        void GetDamage(int damage)
        {
            hp -= damage;
            if (hp <= 0)
                Death();
        }

        void Death()
        {
            SkinnedMeshRenderer[] bodyParts = GetComponentsInChildren<SkinnedMeshRenderer>(); // Search for zombie parts in the array

            for (int i = 0; i < bodyParts.Length; i++)
            {
                bodyParts[i].material.color = Color.red; // Paint the body red
            }

            SwitchRagdoll(true);

            Destroy(gameObject, 5);
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.transform.GetComponent<Player>())
            {
                GetDamage(10);
            }
        }
    }
}