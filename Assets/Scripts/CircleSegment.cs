using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleSegment : MonoBehaviour
{
    private PolygonCollider2D _polygonCollider;
    
    [SerializeField] private int numPoints;
    [SerializeField] private float offset;
    
    private float _innerRadius;
    private float _outerRadius;
    private float _startAngle;
    private float _endAngle;

    // To keep track of the location of the circle Segment
    private int _slice;
    private int _layer;

    public void Initialize(float innerRadius, float outerRadius, float startAngle, float endAngle, int slice, int layer)
    {
        float angleOffset = Mathf.Asin(offset / innerRadius);
        _innerRadius = innerRadius + offset;
        _outerRadius = outerRadius - offset;
        _startAngle = startAngle+angleOffset;
        _endAngle = endAngle-angleOffset;
        _slice = slice;
        _layer = layer;
        
        _polygonCollider = GetComponent<PolygonCollider2D>();
        
        CreateCustomCollider();
    }

    void CreateCustomCollider()
    {
        Vector2[] points = new Vector2[2*(numPoints+1)];
        float unitAngle = (_endAngle - _startAngle) / numPoints;
        float angle = _startAngle;
        for(int i = 0; i <= numPoints; i++)
        {
            points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _innerRadius;
            angle += unitAngle;
        }
        for(int i = numPoints+1; i <= 2*numPoints+1; i++)
        {
            angle -= unitAngle;
            points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _outerRadius;
        }

        _polygonCollider.points = points;
    }


    // Collider only with first segment encountered for now
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Projectile"))
        {   
            bool collision_handled = false; 

            // Access to circleSegmentManager attributes
            CircleSegmentManager circleSegmentManager = GameObject.Find("Planet Bottom").GetComponent<CircleSegmentManager>(); 

            // If the first layer encountered is coloured (not of the color describing an empty block), then the player loses
            if (circleSegmentManager.segmentsOrdered[_slice, circleSegmentManager.nLayer - 1].GetComponent<SpriteRenderer>().color != circleSegmentManager.segmentColors[0]) {
                // ----- TODO: Implement that the player can lose ----- //
                // PlayerLoses();
                collision_handled = true;
                Destroy(other.gameObject);

            // Normal case
            } else if (!collision_handled) {

                // Loop on all segments (layer) of the current slice to find the uncolored segment that is the closest one
                for (int i = circleSegmentManager.nLayer - 2; i >= 0 ; i--) {
                    if (!collision_handled && circleSegmentManager.segmentsOrdered[_slice, i].GetComponent<SpriteRenderer>().color != circleSegmentManager.segmentColors[0]) {

                        // Change colour of block by colour of ball
                        circleSegmentManager.segmentsOrdered[_slice, i+1].GetComponent<SpriteRenderer>().color = other.GetComponent<SpriteRenderer>().color;

                        // Update color Array
                        circleSegmentManager.colorBlocks[_slice, i+1] = other.GetComponent<SpriteRenderer>().color;

                        // ----- TODO: The ball must disappear at the good spot -----
                        Destroy(other.gameObject);

                        // Manage matching of blocks
                        circleSegmentManager.ManageMatching(_slice, i+1);

                        collision_handled = true;
                    }
                }

            // If the player can reach the core of the planet
            } else if (!collision_handled) {

                // ----- TODO: Boss lose a life point ----- //
                // ----- TODO: Update to phase 2 or 3 of Boss fight ----- //

                collision_handled = true;

            }

            


            /*
            GetComponent<SpriteRenderer>().color = other.GetComponent<SpriteRenderer>().color;
            Destroy(other.gameObject);
            */
            

            

        }
    }
}
