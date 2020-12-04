/**
 * File Name: ModelRandomizer.cs
 * Description: Script for randomizing the model of a unit
 * 
 * Authors: Will Lacey
 * Date Created: December 1, 2020
 * 
 * Additional Comments: 
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelRandomizer : MonoBehaviour
{
    [SerializeField] GameObject[] modelPrefabs = null;

    private void Awake()
    {
        foreach (GameObject prefab in modelPrefabs)
        {
            prefab.SetActive(false);
        }

        int index = Random.Range(0, modelPrefabs.Length);

        modelPrefabs[index].SetActive(true);
    }
}
