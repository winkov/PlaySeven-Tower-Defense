using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;
    public bool IsBuildModeActive { get { return true; } }

    void Awake()
    {
        Instance = this;
        Debug.Log("BuildManager Instance set", this);
    }

    void Start()
    {
        Debug.Log("BuildManager active in direct build-spot mode.", this);
    }

    public void ToggleBuildMode()
    {
        Debug.Log("Build mode toggle ignored in direct build-spot mode.", this);
    }

    public void SetBuildMode(bool active)
    {
        Debug.Log("SetBuildMode ignored in direct build-spot mode.", this);
    }
}
