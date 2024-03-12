using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class CarAnimator : MonoBehaviour
{
    public GameObject carPrefab; // Assign in Inspector
    private List<GameObject> currentCars = new List<GameObject>();
    private float frameRate = 0.1f; // 10 fps

    void Start()
    {
        // Add button listener or another trigger to start the animation
    }

    public void StartAnimation()
    {
        StartCoroutine(FetchAndAnimateFrames());
    }

    IEnumerator FetchAndAnimateFrames()
    {
        yield return StartCoroutine(NetworkManager.Instance.GetRequest("frames", response =>
        {
            AnimateFrames(response);
        }));
    }

    void AnimateFrames(string jsonResponse)
    {
        // Adjust this line to match the new JSON structure
        var frames = JsonConvert.DeserializeObject<List<List<CarInfo>>>(jsonResponse);
        StartCoroutine(PlayFrames(frames));
    }

    // Adjust the coroutine to match the new data structure
    IEnumerator PlayFrames(List<List<CarInfo>> frames)
    {
        foreach (var frame in frames)
        {
            ClearCurrentCars();
            foreach (var carInfo in frame)
            {
                Vector3 position = new Vector3(carInfo.position[0], 0, -carInfo.position[1]);
                Quaternion rotation = DirectionToRotation(carInfo.direction);
                GameObject newCar = Instantiate(carPrefab, position, rotation);
                currentCars.Add(newCar);
            }
            yield return new WaitForSeconds(frameRate);
        }
    }

    // Define the CarInfo class to match the structure sent from the server
    [System.Serializable]
    public class CarInfo
    {
        public List<int> position;
        public int direction;
    }

    Quaternion DirectionToRotation(int direction)
    {
        // Convert direction to rotation. Assumes North=0, East=90, South=180, West=270 degrees.
        int angle = 0;
        switch (direction)
        {
            case 1: angle = 180; break; // South
            case 2: angle = 0; break;   // North
            case 3: angle = 90; break;  // East
            case 4: angle = 270; break; // West
            case 5: angle = 0; break;   // Intersection (default to North for now)
        }
        return Quaternion.Euler(0, angle, 0);
    }

    void ClearCurrentCars()
    {
        foreach (GameObject car in currentCars)
        {
            Destroy(car);
        }
        currentCars.Clear();
    }
}
