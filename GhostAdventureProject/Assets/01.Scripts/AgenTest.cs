using UnityEngine;
using UnityEngine.AI;

public class AgentTest : MonoBehaviour
{
    public Transform target;

    private NavMeshAgent _agent;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        // 2D에서는 이부분이 필수
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            MoveToTarget();
    }

    private void MoveToTarget()
    {
        _agent.SetDestination(target.position);
    }
}