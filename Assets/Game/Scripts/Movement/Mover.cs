using UnityEngine;
using UnityEngine.AI;
using RPG.Core;
using RPG.Saving;

namespace RPG.Movement
{
    public class Mover : MonoBehaviour, IAction, ISaveable
    {
        public ParticleSystem effectPrefab; // Ссылка на объект Particle System

        private Animator animator;
        private NavMeshAgent thisNavAgent;
        private ActionScheduler actionScheduler;

        NPCInteractable target;


        void Start()
        {
            if (!thisNavAgent)
                thisNavAgent = GetComponent<NavMeshAgent>();
            if (!actionScheduler)
                actionScheduler = GetComponent<ActionScheduler>();

            animator = GetComponent<Animator>();
        }

        void Update()
        {
            UpdateAnimator();

            // Проверяем, было ли нажатие мыши
            if (Input.GetMouseButtonDown(0))
            {
                CreateEffectAtMousePosition();
            }
            if (target != null)
            {
                if ((transform.position - target.transform.position).magnitude < 1f)
                {
                    target.InteractWithNPC();
                }
            }


            
        }

        public void StartMoveAction(Vector3 pos)
        {
            actionScheduler.StartAction(this);
            MoveTo(pos);
        }

        public void MoveTo(Vector3 pos)
        {
            thisNavAgent.destination = pos;
            thisNavAgent.isStopped = false;
        }

        private void UpdateAnimator()
        {
            if (!thisNavAgent) return;

            Vector3 localVelocity = transform.InverseTransformDirection(thisNavAgent.velocity);
            animator.SetFloat("forwardSpeed", localVelocity.z);
        }

        public void Cancel()
        {
            thisNavAgent.isStopped = true;
        }

        public bool IsAtLocation(float tolerance)
        {
            return Vector3.Distance(thisNavAgent.destination, transform.position) < tolerance;
        }

        public object CaptureState()
        {
            return new SerializableVector3(transform.position);
        }

        public void RestoreState(object state)
        {
            SerializableVector3 oldPos = (SerializableVector3)state;
            actionScheduler = GetComponent<ActionScheduler>();
            thisNavAgent = GetComponent<NavMeshAgent>();
            thisNavAgent.enabled = false;
            transform.position = oldPos.ToVector3();
            thisNavAgent.enabled = true;
        }

        // Метод для создания эффекта на поверхности в указанной позиции
        private void CreateEffectAtMousePosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                CreateEffect(hit.point + new Vector3(0, 0.1f, 0));

                if (hit.transform.CompareTag("Interactable"))
                {
                    target = hit.transform.GetComponent<NPCInteractable>();

                }
                else
                {
                    target = null;
                }
            }
            

        }

        // Метод для создания эффекта в указанной позиции
        private void CreateEffect(Vector3 position)
        {
            Instantiate(effectPrefab, position, Quaternion.identity);
        }
    }
}
