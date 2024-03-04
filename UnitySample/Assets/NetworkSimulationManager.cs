using System;
using UnityEngine;

public class NetworkSimulationManager : MonoBehaviour {

    public static NetworkSimulationManager Instance { get; private set; }

    [SerializeField]
    private float _ping = 100f;
    public float ping => _ping;

    [SerializeField]
    [Range(.0f, 1f)]
    private float reliability = .9f;

    public bool IsNextPacketLost => UnityEngine.Random.Range(0f, 1f) > reliability;

    public void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void OnValidate() {
        _ping = Mathf.Max(0f, _ping);
        reliability = Mathf.Clamp01(reliability);
    }

}
