using System;
using System.Collections.Generic;
using LucasCustomClasses;
using UnityEngine;

public class EntityHitFlash : SingletonBehaviour<EntityHitFlash>
{
    public Material hitMaskMaterialPrefab;
    public float defaultFlashSpeed = 1;
    public float defaultFlashTime = 5;
    private Dictionary<int, RegisteredEntity> registeredEntities = new ();
    private Stack<int> toUnRegister = new();

    private int Counter
    {
        get
        {
            var toReturn = counter;
            counter++;
            return toReturn;
        }
        set => counter = value;
    }
    private int counter = 0;
    
    public class RegisteredEntity
    {
        private struct RendererRegistration
        {
            public Renderer renderer;
            public List<Material> material; // old material(s)

            public RendererRegistration(Renderer renderer)
            {
                this.renderer = renderer;
                material = new List<Material>();
            }
        }
        
        private List<RendererRegistration> rendererRegistrations = new ();
        private Timer flashTimer;
        private Timer timer;
        private int key;
        private bool isDead;
        public Material hitMaskMaterial;
        public float flashSpeed;
        public float flashTime;
        
        public RegisteredEntity(Renderer renderer, float? flashSpeed, float? flashTime, int key)
        {
            AddRegistration(renderer);
            Init(flashSpeed, flashTime, key);
        }

        public RegisteredEntity(Renderer[] renderers, float? flashSpeed, float? flashTime, int key)
        {
            foreach (var renderer in renderers)
            {
                if (renderer == null)
                    continue;
                AddRegistration(renderer);
            }
            Init(flashSpeed, flashTime, key);
        }

        private void Init(float? flashSpeed, float? flashTime, int key)
        {
            SetFlashData(flashSpeed, flashTime);
            hitMaskMaterial = new Material(instance.hitMaskMaterialPrefab);
            this.key = key;
            SetTimer();
        }
        
        private void SetFlashData(float? flashSpeed, float? flashTime)
        {
            try {
                this.flashSpeed = flashSpeed.Value;
                this.flashTime = flashTime.Value;
            } catch (Exception _) {
                // ignored
            }
        }
        
        private void AddRegistration(Renderer renderer)
        {
            var newRegistration = new RendererRegistration(renderer);
            foreach (var material in renderer.sharedMaterials)
            {
                newRegistration.material.Add(new Material(material));
            }
            rendererRegistrations.Add(newRegistration);
        }

        private void SetTimer()
        {
            timer = new Timer(flashTime) { onEnd = UnRegisterFinishedEntity };
            flashTimer = new Timer(flashSpeed) { onEnd = SetHitMaskMaterial };
        }

        private void SetHitMaskMaterial()
        {
            if (!isDead)
            {
                foreach (var registrations in rendererRegistrations)
                {
                    for (var i = 0; i < registrations.renderer.sharedMaterials.Length; i++)
                    {
                        registrations.renderer.sharedMaterials[i] = hitMaskMaterial;
                    }
                }
            }
            
            flashTimer.Reset();
            flashTimer.onEnd = UndoHitMaskMaterial;
        }

        private void UndoHitMaskMaterial()
        {
            ResetMaterials();
            flashTimer.Reset();
            flashTimer.onEnd = SetHitMaskMaterial;
        }

        public void ResetMaterials()
        {
            foreach (var registrations in rendererRegistrations)
            {
                for (var i = 0; i < registrations.renderer.sharedMaterials.Length; i++)
                {
                    registrations.renderer.sharedMaterials[i] = registrations.material[i];
                }
            }
        }
        
        private void UnRegisterFinishedEntity()
        {
            isDead = true;
            instance.UnregisterEntity(key);
        }
        
        public void Update(float dt)
        {
            if (!isDead)
                return;
            flashTimer.Update(dt);
            timer.Update(dt);
        }
    }

    private void Update()
    {
        foreach (var entity in registeredEntities)
        {
            entity.Value?.Update(Time.deltaTime);
        }

        for (int i = 0; i < toUnRegister.Count; i++)
        {
            if (toUnRegister.TryPop(out var key))
                RemoveEntity(key);
        }
    }

    public int RegisterEntity(Renderer renderer, float? flashSpeed = null, float? flashTime = null)
    {
        if (renderer == null)
            return -1;
        flashSpeed ??= instance.defaultFlashSpeed;
        flashTime ??= instance.defaultFlashTime;
        var key = Counter;
        registeredEntities.Add(key, new RegisteredEntity(renderer, flashSpeed, flashTime, key));
        return key;
    }

    public int RegisterEntity(Renderer[] renderers, float? flashSpeed = null, float? flashTime = null)
    {
        if (renderers == null || renderers.Length == 0)
            return -1;
        flashSpeed ??= instance.defaultFlashSpeed;
        flashTime ??= instance.defaultFlashTime;
        var key = Counter;
        registeredEntities.Add(key, new RegisteredEntity(renderers, flashSpeed, flashTime, key));
        return key;
    }

    public RegisteredEntity GetRegisteredEntity(int key)
    {
        return registeredEntities.GetValueOrDefault(key);
    }

    public void UnregisterEntity(int key)
    {
        toUnRegister.Push(key);
    }
    
    public void RemoveEntity(int key)
    {
        var registeredEntity = registeredEntities.GetValueOrDefault(key);
        if (registeredEntity == null)
            return;
        registeredEntities.Remove(key);
        registeredEntity.ResetMaterials();
    }
}