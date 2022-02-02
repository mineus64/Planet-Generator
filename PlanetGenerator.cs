using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class PlanetGenerator : MonoBehaviour
{
    #if !UNITY_SERVER || UNITY_EDITOR
    #region Variables
    [Header("General Settings")]
    public int seed;
    public int resolution;
    public TextureQuality textureQuality;
    [Header("Noise Settings")]
    [Range(0,1)] public float seaLevel = 0.5f;
    public float roughness = 1;
    public Vector3 centre = Vector3.zero;
    [Header("Biome Settings")]
    [Range(-1,1)] public float temperature = 0f;
    [Range(-1,1)] public float rainfall = 0f;
    public float variation = 1;
    [Header("Texture Generation")]
    public Texture2D[] sampleTextures = new Texture2D[6];
    public Material planetMaterial;
    Noise noise;
    Texture2D planetTex;
    Texture2D normalmap;
    #endregion
    #region General Methods
    // Start is called before the first frame update
    void Start()
    {
        // Create a new Noise object
        noise = new Noise(seed);
        // Generate the relevant textures
        planetTex = new Texture2D(resolution, (int)(resolution * 0.5f));

        if (textureQuality == TextureQuality.High) {
            normalmap = new Texture2D(resolution, (int)(resolution * 0.5f));
        }

        // Sample the detailmap to the alpha channel of the texture
        /*
        if (SystemInfo.supportsComputeShaders) {

        }
        else {

        }
        */

        // Generate the texture
        float minHeight = 1;
        float maxHeight = 0;

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < (int)(resolution * 0.5f); y++)
            {
                // Convert the point on the texture plane to a point on a sphere
                // Thanks to https://stackoverflow.com/questions/39440390/deforming-plane-mesh-to-sphere for this one
                float lon = (2 * Mathf.PI) * ((float)x / (float)resolution);
                float lat = (Mathf.PI) * ((float)y / ((float)resolution / 2f));
                // Get the spherical coordinate of this point
                Vector3 samplePoint = new Vector3(
                    Mathf.Cos(lat) * Mathf.Cos(lon),
                    Mathf.Cos(lat) * Mathf.Sin(lon),
                    Mathf.Sin(lat)
                );

                // Sample the noise at the spherical coordinate and store the values
                float temp = Temperature(samplePoint);
                float rain = Rainfall(samplePoint);
                float height = Height(samplePoint);

                if (Random.Range(0f,1f) <= 0.01) {
                    Debug.Log(height);
                }

                if (height < minHeight) {
                    minHeight = height;
                }
                if (height > maxHeight) {
                    maxHeight = height;
                }
                // Use the temperature and rainfall values to select which sample texture to use
                Texture2D sampleTex;

                if (temp > 0.5f) {
                    if (rain > 2/3) {
                        sampleTex = sampleTextures[0];
                    }
                    else if (rain > 1/3) {
                        sampleTex = sampleTextures[1];
                    }
                    else {
                        sampleTex = sampleTextures[2];
                    }
                }
                else {
                    if (rain > 2/3) {
                        sampleTex = sampleTextures[3];
                    }
                    else if (rain > 1/3) {
                        sampleTex = sampleTextures[4];
                    }
                    else {
                        sampleTex = sampleTextures[5];
                    }
                }
                // Get the colour based on the sample and apply it to the texture
                Color sampleCol = sampleTex.GetPixelBilinear(
                    (-height + 1),
                    (((resolution / 2) - (0.5f * (resolution / 2))) / (0.5f * (resolution / 2)))
                );
                //planetTex.SetPixel(x, y, new Color(height * 255, height * 255, height * 255, 1));
                planetTex.SetPixel(x, y, sampleCol);
            }
        }
        // Apply the textures
        planetTex.Apply();
        // Create the normal map, if applicable
        if (textureQuality == TextureQuality.High) {
            normalmap = getNormalMap(planetTex);
        }
        // Apply the texture
        planetMaterial.SetTexture("_MainTex", planetTex);

        if (textureQuality == TextureQuality.High) {
            planetMaterial.SetTexture("_BumpMap", normalmap);
        }

        Debug.Log("Min Height: " + minHeight + " Max Height: " + maxHeight);
    }
    #endregion

    #region Specific Methods
    // Method to generate fine detailmap data for a given point
    public float Height(Vector3 point) 
    {
        return Mathf.Clamp01((noise.Evaluate(point * roughness + centre) + 1) * 0.5f - seaLevel);
    }
    // Method to generate coarse temperature map data for a given point
    public float Temperature(Vector3 point) 
    {
        return Mathf.Clamp01((noise.Evaluate(point * variation + (centre * seed)) + 1) * 0.5f + temperature);
    }
    // Method to generate coarse rainfall map data for a given point
    public float Rainfall(Vector3 point) 
    {
        return Mathf.Clamp01((noise.Evaluate(point * variation + (centre * -seed)) + 1) * 0.5f + rainfall);
    }
    // Method to take in a texture and output a normalmap
    public Texture2D getNormalMap(Texture2D texture, float str = 2.0f)
    {
        Color[] colors = texture.GetPixels();
        Texture2D normal = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
        for (int x = 1; x < texture.width - 1; x++ )
            for (int y = 1; y < texture.height - 1; y++)
            {
                //using Sobel operator
                float tl, t, tr, l, right, bl, bot, br;
                tl = Intensity(texture.GetPixel(x - 1, y - 1));
                t = Intensity(texture.GetPixel(x - 1, y));
                tr = Intensity(texture.GetPixel(x - 1, y + 1));
                right = Intensity(texture.GetPixel(x, y + 1));
                br = Intensity(texture.GetPixel(x + 1, y + 1));
                bot = Intensity(texture.GetPixel(x + 1, y));
                bl = Intensity(texture.GetPixel(x + 1, y - 1));
                l = Intensity(texture.GetPixel(x, y - 1));
 
                //Sobel filter
                float dX = (tr + 2.0f * right + br) - (tl + 2.0f * l + bl);
                float dY = (bl + 2.0f * bot + br) - (tl + 2.0f * t + tr);
                float dZ = 1.0f;
 
                Vector3 vc = new Vector3(str * dX, str * dY, dZ);
                vc.Normalize();
 
                normal.SetPixel(x, y, new Color(0.5f + 0.5f * vc.x, 0.5f + 0.5f * vc.y, 0.5f + 0.5f * vc.z, 0.0f));
            }

        normal.Apply();
        return normal;
    }
    // Method to determine the intensity of a given pixel
    public float Intensity(Color color) {
        return (0.229f * color.r + 0.587f * color.g + 0.114f * color.b);
    }
    #endregion
    #endif
}
#region Enums
public enum TextureQuality 
{
    High,
    Medium,
    Low
}
#endregion