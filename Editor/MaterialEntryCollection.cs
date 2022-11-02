using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fox_score.Find_Incompatible_Shaders
{
    [Serializable]
    public class MaterialEntryCollection
    {
        private List<MaterialEntry> _materialEntries = new List<MaterialEntry>();
        
        private Dictionary<Material, List<MaterialUsage>> _legalUsagesCache = new Dictionary<Material, List<MaterialUsage>>();

        public MaterialEntry[] MaterialEntries => _materialEntries.ToArray();
        
        public void Add(MaterialEntry materialEntry)
        {
            foreach (
                var entry
                in _materialEntries
                    .Where(entry => entry.Target == materialEntry.Target))
            {
                entry.AddUsages(materialEntry);
                return;
            }

            _materialEntries.Add(materialEntry);
        }
        
        public int Count => _materialEntries.Count;
        public MaterialEntry this[int index] => _materialEntries[index];

        public void RemoveAt(int i) => _materialEntries.RemoveAt(i);
        
        public void AddLegalUsage(Material material, MaterialUsage materialUsage)
        {
            if (!_legalUsagesCache.ContainsKey(material))
                _legalUsagesCache.Add(material, new List<MaterialUsage>());
            _legalUsagesCache[material].Add(materialUsage);
        }
        
        public void FlushLegalUsages()
        {
            foreach (var materialEntry in _materialEntries)
            {
                if (!_legalUsagesCache.ContainsKey(materialEntry.Target))
                    continue;
                foreach (var materialUsage in _legalUsagesCache[materialEntry.Target])
                    materialEntry.AddUsage(materialUsage);
            }
            _legalUsagesCache.Clear();
        }
    }
}