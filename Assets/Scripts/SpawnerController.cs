using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerController : MonoBehaviour
{
    //Prefab of a typical ball
    [SerializeField] private GameObject ballPrefab; 

    // Handle pop time
    public float spawnRate = 2f; // One ball every X seconds
    private float nextSpawn = 0; // To set next spawn time
    
    // Possible colors (= same as blocks)  
    private int indexColorToSpawn; // Index of the color of Ball to spawn
    private Color[] colorBalls;

    // Usefull distances for comparisons
    float innerRadius;
    float outerRadius;
    float offset; // To not spawn a ball that collides immediately with the inner or outer circle

    // Mode to generate balls
    public float probabilityAllGenerate = 1; // Probability to generate all ball or nothing when possible, between 0 and 1
                                             // If there is X balls to generate, it will generate X balls or nothing
    public int numberSpawnBalls = 2; // Number of balls to spawn
    public float probabilityGenerate = 1; // Probability to generate a ball, between 0 and 1
    public float multOffset = 2.0f; // Coefficient to multiply by offset to not spawn balls too close
                                   // A coeff of 2 is equal to have twice the radius of a ball between two balls
    public string spawnMode = "lane"; // "random": balls appear at total random on the field 
                                        // "lane": balls appear on a lane 

    // Items usefull to handle lane mode
    public int numberLane = 3; // Number of lanes to play with
    private List<Vector2> laneDistDomains; // To store domain for each lane


    // Start is called before the first frame update
    void Start()
    {
        colorBalls = GameObject.Find("Planet Bottom").GetComponent<CircleSegmentManager>().segmentColors;
        innerRadius = GameObject.Find("Planet Top").GetComponent<InnerCircleCollider>().CurrentRadius * GameObject.Find("Planet Bottom").transform.localScale.x;
        outerRadius = GameObject.Find("Planet Top").GetComponent<InnerCircleCollider>().CurrentRadius * GameObject.Find("Planet Top").transform.localScale.x;
        //Debug.Log("Inner radius: " + innerRadius);
        //Debug.Log("Outer radius: " + outerRadius);

        offset = ballPrefab.GetComponent<CircleCollider2D>().radius;
        Debug.Log("Offset: " + offset);

        // Ensure we have acceptable values for public variables
        if( (probabilityAllGenerate<0) || (probabilityAllGenerate>1)){probabilityAllGenerate = 1;} // Probability must be between 0 and 1
        if ((numberSpawnBalls < 0) || (numberSpawnBalls > 3)){numberSpawnBalls = 1;} // No more than 3 balls at the same time because of size of screen
        if( (probabilityGenerate<0) || (probabilityGenerate>1)){probabilityGenerate = 1;} // Probability must be between 0 and 1
        if ((multOffset < 0) || (multOffset > 3)){multOffset = 2;} // Offset can't bee to high, otherwise there is not enough space to spawn balls on field
        if ((spawnMode != "lane") && (spawnMode != "random")){spawnMode = "lane";} // spawnMode = "lane" by default

        // Precompute lane domains
        if (spawnMode == "lane"){
            if((numberLane<0) || (numberLane>3)){numberLane = 2;} // No more than 3 lanes
            if (numberSpawnBalls > numberLane){numberSpawnBalls = numberLane;} // Can't have more balls than the number of lanes
            laneDistDomains = new List<Vector2>();
            float distStep = (outerRadius - innerRadius) / numberLane;
            for (int i=0; i < numberLane; i++){
                laneDistDomains.Add(new Vector2(innerRadius + i * distStep, innerRadius + (i+1) * distStep));
                //Debug.Log("Distance domain added: " + new Vector2(innerRadius + i * distStep, innerRadius + (i+1) * distStep));
            }
        }
        

    }

    // Update is called once per frame
    void Update()
    {   
        if (Time.time > nextSpawn){

            // Test to check if generation is needed for ALL balls
            if ((Random.Range(0.0f, 1.0f)  <= probabilityAllGenerate)) {
                
                // Loop on all ball to spawn each time
                List<Vector2> impossibleDists = new List<Vector2>(); 
                for (int i=0; i < numberSpawnBalls; i++){
                    
                    // Test to check if generation is needed for EVERY ball
                    if ((Random.Range(0.0f, 1.0f)  <= probabilityGenerate)) {
                        // Get random color to spawn
                        indexColorToSpawn = Random.Range(1, colorBalls.Length - 1);
                        // Debug.Log("Index of color to spawn: " + indexColorToSpawn);
                        ballPrefab.GetComponent<SpriteRenderer>().color = colorBalls[indexColorToSpawn];

                        // Get random position for ball to spawn
                        Vector3 spawnPosition = transform.position - (float)0.8 * transform.right + Random.Range(-2.5f, 2.5f) * transform.up;
                        float dist = Vector3.Distance(GameObject.Find("Planet Bottom").transform.position, spawnPosition); // Distance to center of planet
                        //Debug.Log("Distance to center: " + dist);

                        // -------------------- Random Mode -------------------- //
                        if (spawnMode == "random"){
                            // This position must be between inner and outer circles and not an impossible distance
                            int counter = 0; // Counter to don't enter in an infinite loop in random mode
                            while((dist > outerRadius - 0.5 * offset) || (dist < innerRadius + offset) || !CheckUsabilityDist(dist, impossibleDists)){
                                spawnPosition = transform.position - (float)0.8 * transform.right + Random.Range(-2.5f, 2.5f) * transform.up;
                                dist = Vector3.Distance(GameObject.Find("Planet Bottom").transform.position, spawnPosition);
                                // Debug.Log("Distance to center: " + dist);
                                
                                counter += 1;
                                if (counter == 25){
                                    break;
                                }
                            }
                            
                            // Instantiate a ball only if possible values were found
                            if (counter != 25){
                                // Impossible values are distances in the range of the new position of the ball and the double radius of the ball
                                impossibleDists.Add(new Vector2(dist - (float)multOffset * offset, dist + (float)multOffset * offset));
                                Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
                            }

                        // -------------------- Lane Mode -------------------- //
                        } else if (spawnMode == "lane"){   
                            // Position must in the range of the current lane
                            while((dist< laneDistDomains[i].x + (float)multOffset * offset) || (dist> laneDistDomains[i].y - (float)multOffset * offset)){
                                spawnPosition = transform.position - (float)0.8 * transform.right + Random.Range(-2.5f, 2.5f) * transform.up;
                                dist = Vector3.Distance(GameObject.Find("Planet Bottom").transform.position, spawnPosition);
                            }

                            Instantiate(ballPrefab, spawnPosition, Quaternion.identity);

                        }                         
                    }
                } 

            }
            // Update time before next spawn
            nextSpawn = Time.time + spawnRate;
        }
    }


    private bool CheckUsabilityDist(float dist, List<Vector2> impossibleDists){
        /*
            Inputs:
                * dist: distance from center of circle to check validity
                * impossibleDists: list of intervals of distance that can't be used as the center of a new prefab
            Outputs: boolean true if the dist is not forbidden, false otherwise
        */

        foreach (Vector2 interval in impossibleDists) {
            if( (interval.x <= dist) && (dist <= interval.y)){
                return false;
            }
        }
        return true;
    }

    
    private void OnTriggerEnter2D(Collider2D other)
    {   
        // Remove ball when hitting one
        if (other.CompareTag("Projectile")){ 
                Destroy(other.gameObject);
        }
    }


}
