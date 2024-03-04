using UnityEngine;

public class PlayerModel : MonoBehaviour {

    [SerializeField]
    private Material ownerMaterial;
    [SerializeField]
    private Material clientMaterial;

    private bool isOwner = false;

    private Renderer renderer;

    public Vector3Spring positionSpring = new Vector3Spring() {
        stiffness = 5,
        dampingRatio = .4f
    };

    private void Awake() {
        renderer = GetComponent<Renderer>();
    }


    public void Init(bool isOwner) {
        this.isOwner = isOwner;
    }

    private void Start() {
        renderer.material = isOwner ? ownerMaterial : clientMaterial;

        positionSpring.goal = transform.position;
        positionSpring.value = positionSpring.goal;

    }

    private void Update() {
        positionSpring.Evaluate(Time.deltaTime);
        transform.position = positionSpring.value;
    }

    public void SetPosition(Vector3 position) {
        positionSpring.goal = position;
    }

    private void OnDrawGizmos() {
        if (isOwner)
            return;
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(positionSpring.goal, .2f);
        Gizmos.DrawLine(positionSpring.value, positionSpring.goal);
    }

}
