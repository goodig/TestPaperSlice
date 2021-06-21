using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool flexion { get; set; } //button
    [SerializeField] LineRenderer line;
    [SerializeField] Transform knife;
    Paper paper;
    private void Awake()
    {
        paper = FindObjectOfType<Paper>();
    }

    void Update()
    {

        if (Input.GetMouseButton(0) && !paper.blendActive)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (flexion) //указание изгиба
                {
                    knife.position = hit.point;
                    if(hit.transform.GetComponentInParent<Paper>())
                        CastLineRenderer(hit.point);
                    else
                        DeactivateLine();
                }
                else //поворот кусочков
                {
                    paper.PieceSelect(hit.point);
                }
            }
            
        }
        else
        {
            DeactivateLine();
        }
        
    }

    bool lineActive;
    void CastLineRenderer(Vector3 point)
    {
        if (!lineActive)
        {
            point.y = 1.5f;
            line.SetPosition(0, point);
            line.SetPosition(1, point);
            lineActive = true;
        }
        else
        {
            line.SetPosition(1, point);
        }
    }
    void DeactivateLine()
    {
        lineActive = false;
        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, Vector3.zero);
    }
}
