using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Ghostable : MonoBehaviour
{
    [SerializeField] Dictionary<Material, Material> _regToGhostMaterialMap = new Dictionary<Material, Material>();
    [SerializeField] Dictionary<Material, Material> _ghostToRegMaterialMap = new Dictionary<Material, Material>();

    static Shader shaderURP_Lit;
    static Shader shaderURP_Unlit;

    private void Start()
    {
        if (shaderURP_Lit == null)
        {
            shaderURP_Lit = Shader.Find("Universal Render Pipeline/Lit");
        }

        if (shaderURP_Unlit == null)
        {
            shaderURP_Unlit = Shader.Find("Universal Render Pipeline/Unlit");
        }

        Debug.Log("Start");
        // Get references to materials
        GetMaterialReferences(this.gameObject);
        foreach (Material key in _regToGhostMaterialMap.Keys)
        {
            Debug.Log($"Key: {key.name} Value: {_regToGhostMaterialMap[key].name}");
        }
    }

    private void GetMaterialReferences(GameObject gameObj)
    {
        Debug.Log($"Getting Materials for {gameObj.name}");
        Renderer[] renderers = gameObj.GetComponents<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.materials)
            {
                if (!_regToGhostMaterialMap.ContainsKey(material))
                {
                    if (material.shader.Equals(shaderURP_Lit))
                    {
                        Material unlitMaterial = new Material(material);
                        unlitMaterial.shader = shaderURP_Unlit;
                        unlitMaterial.name += " Unlit";
                        _regToGhostMaterialMap[material] = unlitMaterial;
                        _ghostToRegMaterialMap[unlitMaterial] = material;
                    }
                }
            }
        }

        for (int i = 0; i < gameObj.transform.childCount; i++)
        {
            GetMaterialReferences(gameObj.transform.GetChild(i).gameObject);
        }
    }

    public void GhostMode()
    {
        // If it's a light, turn off.
        if (TryGetComponent<Light>(out Light light))
        {
            light.enabled = false;
        }

        // If it's a rendering object, switch to Unlit shader
        ConvertMaterials(gameObject);

        // If it has emissions, turn them off.
    }

    private void ConvertMaterials(GameObject gameObj)
    {
        Debug.Log($"Changing materials for {gameObj.name}?");
        Renderer[] renderers = gameObj.GetComponents<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                Debug.Log($"Replacing {renderer.materials[i]}");
                renderer.materials[i] = _regToGhostMaterialMap[renderer.materials[i]];
            }
        }

        for (int i = 0; i < gameObj.transform.childCount; i++)
        {
            ConvertMaterials(gameObj.transform.GetChild(i).gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GhostMode();
        }
    }
}