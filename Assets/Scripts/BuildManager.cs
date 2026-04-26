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
    }

    void Start()
    {
        uiManager = FindAnyObjectByType<UIManager>();
        SetBuildMode(false);
    }

    public void ToggleBuildMode()
    {
        SetBuildMode(!buildModeActive);
    }

    public void SetBuildMode(bool active)
    {
        buildModeActive = active;

        if (uiManager != null)
        {
            uiManager.UpdateBuildMode(buildModeActive);
        }
    }
}
