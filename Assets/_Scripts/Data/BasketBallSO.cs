using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "BasketBallSO", menuName = "Scriptable Objects/BasketBallSO")]
public class BasketBallSO : ScriptableObject
{
    [SerializeField]
    public List<BasketBallData> basketBallDataList;
#if UNITY_EDITOR
    // Import from CSV
    [ContextMenu("Import from CSV")]
    public async void ImportFromCSV()
    {
        string soPath = AssetDatabase.GetAssetPath(this);
        string folderPath = Path.GetDirectoryName(soPath);
        string csvPath = Path.Combine(folderPath, "Ball.csv");

        if (!File.Exists(csvPath))
        {
            Debug.LogError("CSV file not found at: " + csvPath);
            return;
        }

        string[] lines = File.ReadAllLines(csvPath);
        if (lines.Length < 2) return;

        basketBallDataList = new List<BasketBallData>();

        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(',');
            if (parts.Length < 10) continue;

            string name = parts[0];
            float mass = float.Parse(parts[1]);
            float radius = float.Parse(parts[2]);
            float drag = float.Parse(parts[3]);
            float density = float.Parse(parts[4]);
            float spinFactor = float.Parse(parts[5]);
            Vector3 spin = new Vector3(float.Parse(parts[6]), float.Parse(parts[7]), float.Parse(parts[8]));
            string matKey = parts[9].Trim();

            Material mat = await Addressables.LoadAssetAsync<Material>(matKey).Task;
            if (mat == null)
            {
                Debug.LogError($"Material with key '{matKey}' not found in Addressables.");
            }

            basketBallDataList.Add(new BasketBallData(name, mass, radius, drag, density, spinFactor, spin, mat));
        }

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        Debug.Log("CSV imported and materials loaded successfully!");
    }
#endif
}

[System.Serializable]
public class BasketBallData
{
    public string name;
    public float ballMass;
    public float ballRadius;
    public float dragCoefficient;
    public float airDensity;
    public float spinFactor;
    public Vector3 initialSpin;

    public Material ballMaterial;

    public BasketBallData(string name, float mass, float radius, float drag, float density, float spinFactor, Vector3 initialSpin, Material ballMaterial)
    {
        this.name = name;
        ballMass = mass;
        ballRadius = radius;
        dragCoefficient = drag;
        airDensity = density;
        this.spinFactor = spinFactor;
        this.initialSpin = initialSpin;
        this.ballMaterial = ballMaterial;
    }
}
