/*using System.Collections.Generic;
using UnityEngine;

public class GenerationManager : MonoBehaviour
{
    
    //TODO Tendria que instanciar todo lo necesario (generadores, grid y demás) al inicio de todo y una vez generado pues solo tener guardados los datos y ya
    // TODO Necesitaría tmb un sistema de guardado y carga de datos para poder guardar los datos de la generación y cargarlos en otro momento
    public List<BaseGenerator> generators = new(); // List of available generators
    public int selectedGeneratorIndex; // Index of the current generator
    
    public BaseGenerator currentGenerator;
    
    public void FindAllGenerators()
    {
        generators = new List<BaseGenerator>(FindObjectsByType<BaseGenerator>(FindObjectsSortMode.None));
    }

    public void Generate()
    {
        if (currentGenerator)
        {
            currentGenerator.RunGeneration();
        }
        else
        {
            Debug.LogWarning("No generator selected.");
        }
    }

    public void SelectGenerator(int index)
    {
        if (index >= 0 && index < generators.Count)
        {
            selectedGeneratorIndex = index;
            currentGenerator = generators[selectedGeneratorIndex];
            Debug.Log($"Selected Generator: {currentGenerator.name}");
        }
        else
        {
            Debug.LogWarning("Invalid generator index.");
        }
    }

    public List<string> GetGeneratorNames()
    {
        var names = new List<string>();
        if (generators != null)
        {
            foreach (var generator in generators)
            {
                if (generator)
                {
                    names.Add(generator.name);
                }
                else
                {
                    Debug.LogWarning("Generator is null.");
                }
            }
        }
        else
        {
            Debug.LogWarning("Generators list is null.");
        }
        return names;
    }
    
}*/