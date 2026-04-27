using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    private bool buildModeActive;
    private UIManager uiManager;

    public bool IsBuildModeActive { get { return buildModeActive; } }

    void Awake()
    {
        Instance = this;
        Debug.Log("BuildManager Instance set", this);
    }

    void Start()
    {
        uiManager = FindAnyObjectByType<UIManager>();
        Debug.Log("BuildManager Start - UIManager found: " + (uiManager != null), this);
        SetBuildMode(false);
    }

    public void ToggleBuildMode()
    {
        Debug.Log("BuildManager ToggleBuildMode called", this);
        SetBuildMode(!buildModeActive);
    }

    public void SetBuildMode(bool active)
    {
        Debug.Log("BuildManager SetBuildMode: " + active, this);
        buildModeActive = active;

        if (uiManager != null)
        {
            uiManager.UpdateBuildMode(buildModeActive);
        }
        else
        {
            Debug.LogWarning("BuildManager: UIManager not found", this);
        }
    }
}
