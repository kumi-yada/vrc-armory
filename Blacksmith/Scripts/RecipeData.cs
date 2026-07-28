using System;

[Serializable]
public class RecipeData
{
    public int id;
    public string recipeName;
    public string description;
    public float optimalFormingHeat;
    public float heatResistance;
    public string[] ingredients;
}
