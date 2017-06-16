﻿// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2017 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
// 
// Notes:
//

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Game;

namespace DaggerfallWorkshop
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class DaggerfallBillboard : MonoBehaviour
    {
        public int FramesPerSecond = 5;     // General FPS
        public bool OneShot = false;        // Plays animation once then destroys GameObject

        [SerializeField]
        BillboardSummary summary = new BillboardSummary();

        public int customArchive = 210;
        public int customRecord = 0;

        Camera mainCamera = null;
        MeshFilter meshFilter = null;
        bool restartAnims = true;
        MeshRenderer meshRenderer;

        // Just using a simple animation speed for simple billboard anims
        // You can adjust this or extend as needed
        const int animalFps = 5;
        const int lightFps = 12;

        public BillboardSummary Summary
        {
            get { return summary; }
        }

        [Serializable]
        public struct BillboardSummary
        {
            public Vector2 Size;                                // Size and scale in world units
            public Rect Rect;                                   // Single UV rectangle for non-atlased materials only
            public Rect[] AtlasRects;                           // Array of UV rectangles for atlased materials only
            public RecordIndex[] AtlasIndices;                  // Indices into UV rect array for atlased materials only, supports animations
            public bool AtlasedMaterial;                        // True if material is part of an atlas
            public bool AnimatedMaterial;                       // True if material uses atlas UV animations (always false for non atlased materials)
            public int CurrentFrame;                            // Current animation frame
            public FlatTypes FlatType;                          // Type of flat
            public EditorFlatTypes EditorFlatType;              // Sub-type of flat when editor/marker
            public bool IsMobile;                               // Billboard is a mobile enemy
            public int Archive;                                 // Texture archive index
            public int Record;                                  // Texture record index
            public int Flags;                                   // NPC Flags found in RMB and RDB NPC data
            public int FactionOrMobileID;                       // FactionID for NPCs, Mobile ID for monsters
            public int NameSeed;                                // NPC name seed
            public uint NativeTextureId;                        // NPC image id from original
            public MobileTypes FixedEnemyType;                  // Type for fixed enemy marker
            public TextureReplacement.CustomBillboard 
                CustomBillboard;                                // Custom textures
        }

        void Start()
        {
            if (Application.isPlaying)
            {
                // Set self inactive if this is an editor marker
                bool showEditorFlats = GameManager.Instance.StartGameBehaviour.ShowEditorFlats;
                if (summary.FlatType == FlatTypes.Editor && !showEditorFlats)
                {
                    this.gameObject.SetActive(false);
                    return;
                }

                // Get component references
                mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
                meshFilter = GetComponent<MeshFilter>();
                meshRenderer = GetComponent<MeshRenderer>();
            }
        }

        void OnDisable()
        {
            restartAnims = true;
        }

        void Update()
        {
            // Restart animation coroutine if not running
            if (restartAnims && summary.AnimatedMaterial)
            {
                StartCoroutine(AnimateBillboard());
                restartAnims = false;
            }

            // Rotate to face camera in game
            if (mainCamera && Application.isPlaying)
            {
                Vector3 viewDirection = -new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);
                transform.LookAt(transform.position + viewDirection);
            }
        }

        IEnumerator AnimateBillboard()
        {
            while (true)
            {
                float speed = FramesPerSecond;
                if (summary.Archive == Utility.TextureReader.AnimalsTextureArchive) speed = animalFps;
                else if (summary.Archive == Utility.TextureReader.LightsTextureArchive) speed = lightFps;
                if (meshFilter != null)
                {
                    summary.CurrentFrame++;

                    // Original Daggerfall textures
                    if (!TextureReplacement.CustomTextureExist(summary.Archive, summary.Record))
                    {
                        if (summary.CurrentFrame >= summary.AtlasIndices[summary.Record].frameCount)
                        {
                            summary.CurrentFrame = 0;
                            if (OneShot)
                                GameObject.Destroy(gameObject);
                        }
                        int index = summary.AtlasIndices[summary.Record].startIndex + summary.CurrentFrame;
                        Rect rect = summary.AtlasRects[index];

                        // Update UVs on mesh
                        Vector2[] uvs = new Vector2[4];
                        uvs[0] = new Vector2(rect.x, rect.yMax);
                        uvs[1] = new Vector2(rect.xMax, rect.yMax);
                        uvs[2] = new Vector2(rect.x, rect.y);
                        uvs[3] = new Vector2(rect.xMax, rect.y);
                        meshFilter.sharedMesh.uv = uvs;
                    }
                    // Custom textures
                    else
                    {
                        // Restart animation or destroy gameobject
                        // The game uses all -and only- textures found on disk, even if they are less or more than vanilla frames
                        if (summary.CurrentFrame >= summary.CustomBillboard.NumberOfFrames)
                        {
                            summary.CurrentFrame = 0;
                            if (OneShot)
                                GameObject.Destroy(gameObject);
                        }

                        // Set Main texture for current frame
                        meshRenderer.materials[0].SetTexture("_MainTex", summary.CustomBillboard.MainTexture[summary.CurrentFrame]);

                        // Set Emission map for current frame
                        if (summary.CustomBillboard.isEmissive)
                            meshRenderer.materials[0].SetTexture("_EmissionMap", summary.CustomBillboard.EmissionMap[summary.CurrentFrame]);
                    }
                }

                yield return new WaitForSeconds(1f / speed);
            }
        }

        /// <summary>
        /// Sets extended data about people billboard from RMB resource data.
        /// </summary>
        /// <param name="person"></param>
        public void SetRMBPeopleData(DFBlock.RmbBlockPeopleRecord person)
        {
            // Add common data
            summary.FactionOrMobileID = person.FactionID;
            summary.FixedEnemyType = MobileTypes.None;
            summary.Flags = person.Flags;
            summary.NativeTextureId = person.TextureBitfield;

            // TEMP: Add name seed
            summary.NameSeed = (int)person.Position;
        }

        /// <summary>
        /// Sets extended data about billboard from RDB flat resource data.
        /// </summary>
        public void SetRDBResourceData(DFBlock.RdbFlatResource resource)
        {
            // Add common data
            summary.Flags = resource.Flags;
            summary.FactionOrMobileID = (int)resource.FactionOrMobileId;
            summary.FixedEnemyType = MobileTypes.None;
            summary.NativeTextureId = resource.TextureBitfield;

            // TEMP: Add name seed
            summary.NameSeed = (int)resource.Position;

            // Set data of fixed mobile types (e.g. non-random enemy spawn)
            if (resource.TextureArchive == 199 && resource.TextureRecord == 16)
            {
                summary.IsMobile = true;
                summary.EditorFlatType = EditorFlatTypes.FixedMobile;
                summary.FixedEnemyType = (MobileTypes)(summary.FactionOrMobileID & 0xff);
            }
        }

        /// <summary>
        /// Sets new Daggerfall material and recreates mesh.
        /// Will use an atlas if specified in DaggerfallUnity singleton.
        /// </summary>
        /// <param name="dfUnity">DaggerfallUnity singleton. Required for content readers and settings.</param>
        /// <param name="archive">Texture archive index.</param>
        /// <param name="record">Texture record index.</param>
        /// <param name="frame">Frame index.</param>
        /// <returns>Material.</returns>
        public Material SetMaterial(int archive, int record, int frame = 0)
        {
            // Get DaggerfallUnity
            DaggerfallUnity dfUnity = DaggerfallUnity.Instance;
            if (!dfUnity.IsReady)
                return null;

            // Get references
            meshRenderer = GetComponent<MeshRenderer>();

            Vector2 size;
            Mesh mesh = null;
            Material material = null;
            if (dfUnity.MaterialReader.AtlasTextures)
            {
                material = dfUnity.MaterialReader.GetMaterialAtlas(
                    archive,
                    0,
                    4,
                    2048,
                    out summary.AtlasRects,
                    out summary.AtlasIndices,
                    4,
                    true,
                    0,
                    false,
                    true);
                mesh = dfUnity.MeshReader.GetBillboardMesh(
                    summary.AtlasRects[summary.AtlasIndices[record].startIndex],
                    archive,
                    record,
                    out size);
                summary.AtlasedMaterial = true;
                if (summary.AtlasIndices[record].frameCount > 1)
                    summary.AnimatedMaterial = true;
                else
                    summary.AnimatedMaterial = false;
            }
            else
            {
                material = dfUnity.MaterialReader.GetMaterial(
                    archive,
                    record,
                    frame,
                    0,
                    out summary.Rect,
                    4,
                    true,
                    true);
                mesh = dfUnity.MeshReader.GetBillboardMesh(
                    summary.Rect,
                    archive,
                    record,
                    out size);
                summary.AtlasedMaterial = false;
                summary.AnimatedMaterial = false;
            }

            // Set summary
            summary.FlatType = MaterialReader.GetFlatType(archive);
            summary.Archive = archive;
            summary.Record = record;
            summary.Size = size;

            // Set editor flat types
            if (summary.FlatType == FlatTypes.Editor)
                summary.EditorFlatType = MaterialReader.GetEditorFlatType(summary.Record);

            // Set NPC flat type based on archive
            // This is just a hack for now while performing
            // more research into NPC names
            if (summary.Archive == 334 ||                               // Daggerfall people
                summary.Archive == 346 ||                               // Wayrest people
                summary.Archive == 357 ||                               // Sentinel people
                summary.Archive >= 175 && summary.Archive <= 184)       // Other people
            {
                summary.FlatType = FlatTypes.NPC;
            }

            // Assign mesh and material
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            Mesh oldMesh = meshFilter.sharedMesh;
            if (mesh)
            {
                meshFilter.sharedMesh = mesh;
                meshRenderer.sharedMaterial = material;
            }
            if (oldMesh)
            {
                // The old mesh is no longer required
#if UNITY_EDITOR
                DestroyImmediate(oldMesh);
#else
                Destroy(oldMesh);
#endif
            }

            // Standalone billboards never cast shadows
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;

            // Add NPC trigger collider
            if (summary.FlatType == FlatTypes.NPC)
            {
                Collider col = gameObject.AddComponent<BoxCollider>();
                col.isTrigger = true;
            }

            return material;
        }

        /// <summary>
        /// Set custom textures and properties for billboard.
        /// </summary>
        public void SetCustomMaterial(TextureReplacement.CustomBillboard customBillboard)
        {
            // Get DaggerfallUnity
            DaggerfallUnity dfUnity = DaggerfallUnity.Instance;
            if (!dfUnity.IsReady)
                return;

            // Save summary
            summary.CustomBillboard = customBillboard;
        }

        /// <summary>
        /// Set new Y size for correct positioning.
        /// </summary>
        /// <param name="archive">Texture archive index.</param>
        /// <param name="record">Texture record index.</param>
        /// <param name="scaleY">height scale</param>
        public void SetCustomSize(int archive, int record, float scaleY)
        {
            summary.Size.y *= scaleY;
        }

        /// <summary>
        /// Aligns billboard to centre of base, rather than exact centre.
        /// Must have already set material using SetMaterial() for billboard dimensions to be known.
        /// </summary>
        public void AlignToBase()
        {
            // Calcuate offset for correct positioning in scene
            Vector3 offset = Vector3.zero;
            offset.y = (summary.Size.y / 2);
            transform.position += offset;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Rotate billboards to face editor camera while game not running.
        /// </summary>
        public void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                UnityEditor.SceneView sceneView = GetActiveSceneView();
                if (sceneView)
                {
                    // Editor camera stands in for player camera in edit mode
                    Vector3 viewDirection = -new Vector3(sceneView.camera.transform.forward.x, 0, sceneView.camera.transform.forward.z);
                    transform.LookAt(transform.position + viewDirection);
                }
            }
        }

        private SceneView GetActiveSceneView()
        {
            // Return the focused window if it is a SceneView
            if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType() == typeof(SceneView))
                return (SceneView)EditorWindow.focusedWindow;

            return null;
        }
#endif
    }
}