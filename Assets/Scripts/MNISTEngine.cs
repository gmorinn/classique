using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;
using System.Linq;

/*
 *  Neural net engine and handles the inference.
 *   - Shifts the image to the center for better inference. 
 *   (The model was trained on images centered in the texture this will give better results)
 *  - recentering of the image is also done using special operations on the GPU
 *   
 */


public struct Bounds
{
    public int left;
    public int right;
    public int top;
    public int bottom;
}

public class MNISTEngine : MonoBehaviour
{
    public ModelAsset mnistONNX;

    
    IWorker engine;

    
    static Unity.Sentis.BackendType backendType = Unity.Sentis.BackendType.GPUCompute;

    
    const int imageWidth = 28;

    
    TensorFloat inputTensor = null;

    
    Ops ops;

    Camera lookCamera;


    void Start()
    {
        
        Model model = ModelLoader.Load(mnistONNX);
        
        engine = WorkerFactory.CreateWorker(backendType, model);

        
        ops = WorkerFactory.CreateOps(backendType, null);

        
        lookCamera = Camera.main;
    }

    
    public (float, int) GetMostLikelyDigitProbability(Texture2D drawableTexture)
    {
        inputTensor?.Dispose();

        
        inputTensor = TextureConverter.ToTensor(drawableTexture, imageWidth, imageWidth, 1);
        
        
        engine.Execute(inputTensor);
        
        
        TensorFloat result = engine.PeekOutput() as TensorFloat;
        
        
        var probabilities = ops.Softmax(result);
        var indexOfMaxProba = ops.ArgMax(probabilities, -1, false);
        
        
        probabilities.MakeReadable();
        indexOfMaxProba.MakeReadable();

        var predictedNumber = indexOfMaxProba[0];
        var probability = probabilities[predictedNumber];

        return (probability, predictedNumber);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MouseClicked();
        }
        else if (Input.GetMouseButton(0))
        {
            MouseIsDown();
        }
    }

    
    void MouseClicked()
    {
        Ray ray = lookCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.name == "Screen")
        {
            Panel panel = hit.collider.GetComponentInParent<Panel>();
            if (!panel) return;
            panel.ScreenMouseDown(hit);
        }
    }

    
    void MouseIsDown()
    {
        Ray ray = lookCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.name == "Screen")
        {
            Panel panel = hit.collider.GetComponentInParent<Panel>();
            if (!panel) return;
            panel.ScreenGetMouse(hit);
        }
    }
   
    
    private void OnDestroy()
    {
        inputTensor?.Dispose();
        engine?.Dispose();
        ops?.Dispose();
    }

}
