using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;


public class SpellBuilder 
{
    private const string ImplementedBaseSpellId = "arcane_bolt";
    private Dictionary<string, SpellData> spells;
    private List<string> baseSpellIds;
    private List<string> modifierSpellIds;

    public Spell Build(SpellCaster owner)
    {
        if (baseSpellIds.Count == 0)
        {
            Debug.LogError("No base spells found in spells.json");
            return new Spell(owner);
        }

        string baseSpellId = ImplementedBaseSpellId;
        SpellData baseSpellData = GetSpellData(baseSpellId);

        if (baseSpellData == null)
        {
            return new Spell(owner);
        }

        string modifierSpellId = null;
        SpellData modifierSpellData = null;

        if (modifierSpellIds.Count > 0)
        {
            modifierSpellId = GetRandomSpellId(modifierSpellIds);
            modifierSpellData = GetSpellData(modifierSpellId);
        }

        Spell spell = new Spell(owner, baseSpellId, baseSpellData, modifierSpellId, modifierSpellData);
        Debug.Log("Built random spell: " + spell.GetName());
        return spell;
    }

    public SpellData GetSpellData(string spellId)
    {
        if (!spells.ContainsKey(spellId))
        {
            Debug.LogError("Spell not found in spells.json: " + spellId);
            return null;
        }

        return spells[spellId];
    }

    public List<string> GetBaseSpellIds()
    {
        return baseSpellIds;
    }

    public List<string> GetModifierSpellIds()
    {
        return modifierSpellIds;
    }

    private string GetRandomSpellId(List<string> spellIds)
    {
        return spellIds[Random.Range(0, spellIds.Count)];
    }

    public SpellBuilder()
    {
        string json = File.ReadAllText(Application.dataPath + "/Resources/spells.json");
        spells = JsonConvert.DeserializeObject<Dictionary<string, SpellData>>(json);

        if (spells == null)
        {
            Debug.LogError("Could not read spells.json");
            spells = new Dictionary<string, SpellData>();
        }

        baseSpellIds = new List<string>();
        modifierSpellIds = new List<string>();

        foreach (KeyValuePair<string, SpellData> entry in spells)
        {
            if (entry.Value.IsBaseSpell())
            {
                baseSpellIds.Add(entry.Key);
            }
            else if (entry.Value.IsModifier())
            {
                modifierSpellIds.Add(entry.Key);
            }
        }

        LogLoadedSpellData();
    }

    private void LogLoadedSpellData()
    {
        Debug.Log("Loaded " + spells.Count + " spell entries from spells.json");
        Debug.Log("Base spells (" + baseSpellIds.Count + "): " + string.Join(", ", baseSpellIds.ToArray()));
        Debug.Log("Modifiers (" + modifierSpellIds.Count + "): " + string.Join(", ", modifierSpellIds.ToArray()));

        SpellData arcaneBolt = GetSpellData("arcane_bolt");
        if (arcaneBolt != null)
        {
            Debug.Log("Arcane Bolt check: damage " + arcaneBolt.damage.amount + ", mana " + arcaneBolt.mana_cost + ", trajectory " + arcaneBolt.projectile.trajectory);
        }
    }

}

public class SpellData
{
    public string name;
    public string description;
    public int icon;

    public string N;
    public string spray;
    public SpellDamageData damage;
    public string secondary_damage;
    public string mana_cost;
    public string cooldown;
    public SpellProjectileData projectile;
    public SpellProjectileData secondary_projectile;

    public string damage_multiplier;
    public string mana_multiplier;
    public string speed_multiplier;
    public string cooldown_multiplier;
    public string delay;
    public string angle;
    public string projectile_trajectory;
    public string mana_adder;
    public string pierce;

    public bool IsBaseSpell()
    {
        return projectile != null;
    }

    public bool IsModifier()
    {
        return projectile == null;
    }
}

public class SpellDamageData
{
    public string amount;
    public string type;
}

public class SpellProjectileData
{
    public string trajectory;
    public string speed;
    public int sprite;
    public string lifetime;
}
