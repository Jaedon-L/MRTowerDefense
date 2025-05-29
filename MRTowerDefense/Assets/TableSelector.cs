using System;
using UnityEngine;
using Oculus.Interaction;
using System.Collections;

[RequireComponent(typeof(RayInteractor))]
public class TableSelector : MonoBehaviour
{
    [Header("Input & Ray")]
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;
    public float pinchThreshold = 0.5f;

    [Header("Layers")]
    public LayerMask tableLayer; // assign to your table(s)' layer

    private RayInteractor _ray;
    private bool          _selecting = false;

    public event Action<PatternSpawner> OnTableSelected;

    void Awake()
    {
        _ray = GetComponent<RayInteractor>();
        _ray.gameObject.SetActive(false);
    }

void Update()
{
    // if (!_selecting) return;

    // float pinch = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller);
    // if (pinch <= pinchThreshold) return;

    // // Instead of SurfaceHit.Interactable, use SelectedInteractable:
    // var tableInteractable = _ray.SelectedInteractable;
    // if (tableInteractable != null 
    //     && ((1 << tableInteractable.gameObject.layer) & tableLayer) != 0)
    // {
    //     var spawner = tableInteractable.GetComponent<PatternSpawner>();
    //     if (spawner != null)
    //     {
    //         EndTableSelection();
    //         spawner.StartSpawn();
    //         OnTableSelected?.Invoke(spawner);
    //     }
    // }
}
    /// <summary>
    /// Call this when the Start Game button is pressed.
    /// </summary>
    public void BeginTableSelection()
    {
        _selecting = true;
        _ray.gameObject.SetActive(true);
        StartCoroutine(EndTableSelection()); 
    }

     IEnumerator EndTableSelection()
    {
        yield return new WaitForSeconds(5f);
        _selecting = false;
        _ray.gameObject.SetActive(false);
    }
}
