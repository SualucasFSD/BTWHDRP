using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractManager: MonoBehaviour
{
        public static InteractManager Instance;
        
        private List<Interact> _interactables = new List<Interact>();
        private Canvas _canvasInteract;
        [SerializeField] protected Canvas _prefab;
        private Interact _current;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        _canvasInteract = Instantiate(_prefab);
        _canvasInteract.transform.position = Vector3.up * -10000f;
        }

        private void Start()
        {
            EventManager.Suscribe(EventManager.KindOfEvent.OnPjChangePosition, UpdateInteract);
        }

        private void UpdateInteract(params object[] p)
        {
            Vector3 pjPos = (Vector3)p[0];
            float minDistance = Mathf.Infinity;
            Interact nearest = null;

            foreach (Interact obj in _interactables)
            {
                float dist = Vector3.Distance(obj.transform.position, pjPos);
                if (dist < 3 && dist < minDistance)
                {
                    minDistance = dist;
                    nearest = obj;
                }
            }

            _current = nearest;

            if (_current != null)
            {
                _canvasInteract.transform.position = _current.transform.position + _current._offset;
                _canvasInteract.transform.LookAt(Camera.main.transform.position);
               if (Input.GetButtonDown("Interact"))
               {
                 _current.Interacting();
               }
            }
        }

        public void AddInteract(Interact p)
        {
            if (!_interactables.Contains(p))
            {
                _interactables.Add(p);
            }
        }

        public void RemoveInteract(Interact p)
        {
            _interactables.Remove(p);
        }

        private void OnDestroy()
        {
            EventManager.Unscribe(EventManager.KindOfEvent.OnPjChangePosition, UpdateInteract);
        }
}
