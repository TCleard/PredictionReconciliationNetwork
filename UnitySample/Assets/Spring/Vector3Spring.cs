using System;
using UnityEngine;

[Serializable]
public class Vector3Spring {

    public Vector3 _value;
    public Vector3 value {
        get => _value;
        set {
            if (_value != value) {
                _value = value;
                onValueChanged?.Invoke(value);
            }
        }
    }

    public Vector3 goal;

    [SerializeField]
    private float _stiffness = 1f;
    public float stiffness {
        get => _stiffness;
        set => _stiffness = Mathf.Max(0f, value);
    }

    [SerializeField]
    private float _dampingRatio = .2f;
    public float dampingRatio {
        get => _dampingRatio;
        set { _dampingRatio = Mathf.Clamp01(value); }
    }

    private Vector3 offset;

    private Vector3 suspensionForce;
    private Vector3 damperForce;
    public Vector3 force { get; private set; }

    public event Action<Vector3> onValueChanged;

    public void Evaluate(float deltaTime) {

        offset = goal - value;

        suspensionForce = stiffness * offset;
        damperForce = dampingRatio * -force;
        force += suspensionForce + damperForce;

        value += force * deltaTime;

    }

}