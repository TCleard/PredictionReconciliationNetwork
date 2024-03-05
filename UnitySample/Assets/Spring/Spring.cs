using System;
using UnityEngine;

[Serializable]
public class Spring {
    public float _value;
    public float value {
        get => _value;
        set {
            if (_value != value) {
                _value = value;
                onValueChanged?.Invoke(value);
            }
        }
    }

    public float goal = 0f;

    [SerializeField]
    private float _stiffness = 5f;
    public float stiffness {
        get => _stiffness;
        set => _stiffness = Mathf.Max(0f, value);
    }

    [SerializeField]
    private float _dampingRatio = .3f;
    public float dampingRatio {
        get => _dampingRatio;
        set { _dampingRatio = Mathf.Clamp01(value); }
    }

    private float offset;

    private float suspensionForce;
    private float damperForce;
    public float force { get; private set; }

    public event Action<float> onValueChanged;

    public void Evaluate(float deltaTime) {

        offset = goal - value;

        suspensionForce = stiffness * offset * deltaTime;
        damperForce = dampingRatio * -force;
        force += suspensionForce + damperForce;

        value += force;

    }

}
