﻿using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Mochie;

public class WaterEditor : ShaderGUI {

    GUIContent texLabel = new GUIContent("Base Color");
    GUIContent normalLabel = new GUIContent("Normal Map");
	GUIContent flowLabel = new GUIContent("Flow Map");
	GUIContent noiseLabel = new GUIContent("Noise Texture");
	GUIContent foamLabel = new GUIContent("Foam Texture");
	GUIContent cubeLabel = new GUIContent("Cubemap");

	Dictionary<Action, GUIContent> baseTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> norm0TabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> norm1TabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> flowTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> vertTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> causticsTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> fogTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> foamTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> edgeFadeTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> reflSpecTabButtons = new Dictionary<Action, GUIContent>();
	Dictionary<Action, GUIContent> rainTabButtons = new Dictionary<Action, GUIContent>();

    static Dictionary<Material, Toggles> foldouts = new Dictionary<Material, Toggles>();
    Toggles toggles = new Toggles(new string[] {
			"BASE", 
			"PRIMARY NORMAL", 
			"SECONDARY NORMAL",
			"REFLECTIONS & SPECULAR", 
			"FLOW MAPPING", 
			"VERTEX OFFSET",
			"CAUSTICS",
			"DEPTH FOG",
			"FOAM",
			"EDGE FADE",
			"RAIN"
	}, 0);

    string header = "WaterHeader_Pro";
	string versionLabel = "v1.5";

	MaterialProperty _Color = null;
	MaterialProperty _MainTex = null;
	MaterialProperty _MainTexScroll = null;
	MaterialProperty _DistortionStrength = null;
	MaterialProperty _Roughness = null;
	MaterialProperty _Metallic = null;
	MaterialProperty _Opacity = null;
	MaterialProperty _Reflections = null;
	MaterialProperty _ReflStrength = null;
	MaterialProperty _Specular = null;
	MaterialProperty _SpecStrength = null;
	MaterialProperty _CullMode = null;
	MaterialProperty _ZWrite = null;
	MaterialProperty _BaseColorDistortionStrength = null;

	MaterialProperty _NormalMap0 = null;
	MaterialProperty _NormalStr0 = null;
	MaterialProperty _NormalMapScale0 = null;
	MaterialProperty _Rotation0 = null;
	MaterialProperty _NormalMapScroll0 = null;

	MaterialProperty _Normal1Toggle = null;
	MaterialProperty _NormalMap1 = null;
	MaterialProperty _NormalStr1 = null;
	MaterialProperty _NormalMapScale1 = null;
	MaterialProperty _Rotation1 = null;
	MaterialProperty _NormalMapScroll1 = null;

	MaterialProperty _FlowToggle = null;
	MaterialProperty _FlowMap = null;
	MaterialProperty _FlowSpeed = null;
	MaterialProperty _FlowStrength = null;
	MaterialProperty _FlowMapScale = null;

	MaterialProperty _NoiseTex = null;
	MaterialProperty _NoiseTexScale = null;
	MaterialProperty _NoiseTexScroll = null;
	MaterialProperty _NoiseTexBlur = null;
	MaterialProperty _WaveHeight = null;
	MaterialProperty _Offset = null;
	MaterialProperty _VertOffsetMode = null;
	MaterialProperty _WaveSpeedGlobal = null;
	MaterialProperty _WaveStrengthGlobal = null;
	MaterialProperty _WaveScaleGlobal = null;
	MaterialProperty _WaveSpeed0 = null;
	MaterialProperty _WaveSpeed1 = null;
	MaterialProperty _WaveSpeed2 = null;
	MaterialProperty _WaveScale0 = null;
	MaterialProperty _WaveScale1 = null;
	MaterialProperty _WaveScale2 = null;
	MaterialProperty _WaveStrength0 = null;
	MaterialProperty _WaveStrength1 = null;
	MaterialProperty _WaveStrength2 = null;
	MaterialProperty _WaveDirection0 = null;
	MaterialProperty _WaveDirection1 = null;
	MaterialProperty _WaveDirection2 = null;
	MaterialProperty _Turbulence = null;
	MaterialProperty _TurbulenceSpeed = null;
	MaterialProperty _TurbulenceScale = null;

	MaterialProperty _CausticsToggle = null;
	MaterialProperty _CausticsOpacity = null;
	MaterialProperty _CausticsScale = null;
	MaterialProperty _CausticsSpeed = null;
	MaterialProperty _CausticsFade = null;
	MaterialProperty _CausticsDisp = null;
	MaterialProperty _CausticsDistortion = null;
	MaterialProperty _CausticsDistortionScale = null;
	MaterialProperty _CausticsDistortionSpeed = null;
	MaterialProperty _CausticsRotation = null;

	MaterialProperty _FogToggle = null;
	MaterialProperty _FogTint = null;
	MaterialProperty _FogPower = null;

	MaterialProperty _FoamToggle = null;
	MaterialProperty _FoamTex = null;
	MaterialProperty _FoamNoiseTex = null;
	MaterialProperty _FoamTexScale = null;
	MaterialProperty _FoamRoughness = null;
	MaterialProperty _FoamColor = null;
	MaterialProperty _FoamPower = null;
	MaterialProperty _FoamOpacity = null;
	MaterialProperty _FoamCrestStrength = null;
	MaterialProperty _FoamCrestThreshold = null;
	MaterialProperty _FoamNoiseTexScroll = null;
	MaterialProperty _FoamNoiseTexCrestStrength = null;
	MaterialProperty _FoamNoiseTexStrength = null;
	MaterialProperty _FoamNoiseTexScale = null;

	MaterialProperty _EdgeFadeToggle = null;
	MaterialProperty _EdgeFadePower = null;
	MaterialProperty _EdgeFadeOffset = null;
	MaterialProperty _SSRStrength = null;
	MaterialProperty _SSR = null;
	MaterialProperty _EdgeFadeSSR = null;
	MaterialProperty _Normal0StochasticToggle = null;
	MaterialProperty _Normal1StochasticToggle = null;
	MaterialProperty _FoamStochasticToggle = null;
	MaterialProperty _FoamTexScroll = null;
	MaterialProperty _BaseColorStochasticToggle = null;
	MaterialProperty _NormalMapOffset1 = null;
	MaterialProperty _FoamOffset = null;
	MaterialProperty _NormalMapOffset0 = null;
	MaterialProperty _BaseColorOffset = null;
	MaterialProperty _FoamDistortionStrength = null;
	MaterialProperty _VertRemapMin = null;
	MaterialProperty _VertRemapMax = null;

	MaterialProperty _LightDir = null;
	MaterialProperty _SpecTint = null;

	MaterialProperty _ReflCube = null;
	MaterialProperty _ReflTint = null;
	MaterialProperty _ReflCubeRotation = null;

	MaterialProperty _RainToggle = null;
	MaterialProperty _RippleScale = null;
	MaterialProperty _RippleSpeed = null;
	MaterialProperty _RippleStr = null;
	MaterialProperty _FoamNormalToggle = null;
	MaterialProperty _FoamNormalStrength = null;

    BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
	bool m_FirstTimeApply = true;

    public override void OnGUI(MaterialEditor me, MaterialProperty[] props) {
        if (!me.isVisible)
            return;

		ClearDictionaries();

        foreach (var property in GetType().GetFields(bindingFlags)){
            if (property.FieldType == typeof(MaterialProperty))
                property.SetValue(this, FindProperty(property.Name, props));
        }
        Material mat = (Material)me.target;
        if (m_FirstTimeApply){
			m_FirstTimeApply = false;
        }

		header = "WaterHeader_Pro";
		if (!EditorGUIUtility.isProSkin){
			header = "WaterHeader";
		}

        Texture2D headerTex = (Texture2D)Resources.Load(header, typeof(Texture2D));
		Texture2D collapseIcon = (Texture2D)Resources.Load("CollapseIcon", typeof(Texture2D));

        GUILayout.Label(headerTex);
		MGUI.Space4();

		if (!foldouts.ContainsKey(mat))
			foldouts.Add(mat, toggles);

        EditorGUI.BeginChangeCheck(); {

            // Surface
			baseTabButtons.Add(()=>{Toggles.CollapseFoldouts(mat, foldouts, 1);}, MGUI.collapseLabel);
			baseTabButtons.Add(()=>{ResetSurface();}, MGUI.resetLabel);
			Action surfaceTabAction = ()=>{
				MGUI.PropertyGroup( () => {
					me.RenderQueueField();
					me.ShaderProperty(_CullMode, "Culling Mode");
					me.ShaderProperty(_ZWrite, "ZWrite");
					me.ShaderProperty(_Opacity, "Opacity");
					me.ShaderProperty(_DistortionStrength, "Distortion Strength");
					MGUI.Space2();
				});
				MGUI.PropertyGroup( () => {
					me.TexturePropertySingleLine(texLabel, _MainTex, _Color, _BaseColorStochasticToggle);
					MGUI.TexPropLabel(Tips.stochasticLabel, 172);
					MGUI.TextureSOScroll(me, _MainTex, _MainTexScroll);
					me.ShaderProperty(_BaseColorOffset, Tips.parallaxOffsetLabel);
					me.ShaderProperty(_BaseColorDistortionStrength, "Distortion Strength");
				});
				MGUI.DisplayInfo("   This shader requires a \"Depth Light\" prefab be present in the scene.\n   (Found in: Assets/Mochie/Unity/Prefabs)");
			};
			Foldouts.Foldout("BASE", foldouts, baseTabButtons, mat, me, surfaceTabAction);

			// Primary Normal
			norm0TabButtons.Add(()=>{ResetPrimaryNormal();}, MGUI.resetLabel);
			Action norm0TabAction = ()=>{
				MGUI.Space4();
				MGUI.PropertyGroup( () => {
					me.TexturePropertySingleLine(Tips.waterNormalMap, _NormalMap0, _Normal0StochasticToggle);
					MGUI.TexPropLabel(Tips.stochasticLabel, 172);
					me.ShaderProperty(_NormalStr0, "Strength");
					MGUI.Vector2Field(_NormalMapScale0, "Scale");
					MGUI.Vector2Field(_NormalMapScroll0, "Scrolling");
					me.ShaderProperty(_Rotation0, "Rotation");
					me.ShaderProperty(_NormalMapOffset0, Tips.parallaxOffsetLabel);
				});
			};
			Foldouts.Foldout("PRIMARY NORMAL", foldouts, norm0TabButtons, mat, me, norm0TabAction);

			// Secondary Normal
			norm1TabButtons.Add(()=>{ResetSecondaryNormal();}, MGUI.resetLabel);
			Action norm1TabAction = ()=>{
				me.ShaderProperty(_Normal1Toggle, "Enable");
				MGUI.Space4();
				MGUI.PropertyGroup( () => {
					MGUI.ToggleGroup(_Normal1Toggle.floatValue == 0);
					me.TexturePropertySingleLine(Tips.waterNormalMap, _NormalMap1, _Normal1StochasticToggle);
					MGUI.TexPropLabel(Tips.stochasticLabel, 172);
					me.ShaderProperty(_NormalStr1, "Strength");
					MGUI.Vector2Field(_NormalMapScale1, "Scale");
					MGUI.Vector2Field(_NormalMapScroll1, "Scrolling");
					me.ShaderProperty(_Rotation1, "Rotation");
					me.ShaderProperty(_NormalMapOffset1, Tips.parallaxOffsetLabel);
					MGUI.ToggleGroupEnd();
				});
			};
			Foldouts.Foldout("SECONDARY NORMAL", foldouts, norm1TabButtons, mat, me, norm1TabAction);

			// Reflections & Specular
			reflSpecTabButtons.Add(()=>{ResetReflSpec();}, MGUI.resetLabel);
			Action reflSpecTabAction = ()=>{
				MGUI.Space4();
				me.ShaderProperty(_Roughness, Tips.waterRoughness);
				me.ShaderProperty(_Metallic, Tips.waterMetallic);
				MGUI.Space8();
				me.ShaderProperty(_Reflections, "Reflections");
				MGUI.PropertyGroup( () => {
					MGUI.ToggleGroup(_Reflections.floatValue == 0);
					if (_Reflections.floatValue == 2){
						me.TexturePropertySingleLine(cubeLabel, _ReflCube);
						MGUI.Vector3Field(_ReflCubeRotation, "Rotation", false);
					}
					me.ShaderProperty(_ReflStrength, "Strength");
					
					MGUI.ToggleFloat(me, "Screenspace Reflections", _SSR, _SSRStrength);
					if (_SSR.floatValue > 0)
						me.ShaderProperty(_EdgeFadeSSR, "Edge Fade");
					me.ShaderProperty(_ReflTint, "Tint");
					MGUI.ToggleGroupEnd();
				});
				MGUI.Space8();
				me.ShaderProperty(_Specular, "Specular");
				MGUI.PropertyGroup( ()=>{
					MGUI.ToggleGroup(_Specular.floatValue == 0);
					if (_Specular.floatValue == 2){
						MGUI.Vector3Field(_LightDir, "Light Direction", false);
					}
					me.ShaderProperty(_SpecStrength, "Strength");
					me.ShaderProperty(_SpecTint, "Tint");
					MGUI.ToggleGroupEnd();
				});

			};
			Foldouts.Foldout("REFLECTIONS & SPECULAR", foldouts, reflSpecTabButtons, mat, me, reflSpecTabAction);

			// Flow Mapping
			flowTabButtons.Add(()=>{ResetFlowMapping();}, MGUI.resetLabel);
			Action flowTabAction = ()=>{
				me.ShaderProperty(_FlowToggle, "Enable");
				MGUI.Space4();
				MGUI.PropertyGroup( () => {
					MGUI.ToggleGroup(_FlowToggle.floatValue == 0);
					me.TexturePropertySingleLine(flowLabel, _FlowMap);
					MGUI.Vector2Field(_FlowMapScale, "Scale");
					me.ShaderProperty(_FlowSpeed, "Speed");
					me.ShaderProperty(_FlowStrength, "Strength");
					MGUI.ToggleGroupEnd();
				});
			};
			Foldouts.Foldout("FLOW MAPPING", foldouts, flowTabButtons, mat, me, flowTabAction);

			// Vertex Offset
			vertTabButtons.Add(()=>{ResetVertOffset();}, MGUI.resetLabel);
			Action vertTabAction = ()=>{
				me.ShaderProperty(_VertOffsetMode, "Mode");
				MGUI.Space4();
				MGUI.ToggleGroup(_VertOffsetMode.floatValue == 0);
				if (_VertOffsetMode.floatValue == 1){
					MGUI.PropertyGroup( () => {
						me.TexturePropertySingleLine(noiseLabel, _NoiseTex);
						me.ShaderProperty(_NoiseTexBlur, "Blur");
						MGUI.Vector2Field(_NoiseTexScale, "Scale");
						MGUI.Vector2Field(_NoiseTexScroll, "Scrolling");
					});
					MGUI.PropertyGroup( () => {
						MGUI.Vector3Field(_Offset, "Strength", false);
						me.ShaderProperty(_WaveHeight, "Strength Multiplier");
						MGUI.SliderMinMax(_VertRemapMin, _VertRemapMax, -1f, 1f, "Remap", 1);
					});
				}
				else if (_VertOffsetMode.floatValue == 2){
					MGUI.BoldLabel("Global");
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_WaveStrengthGlobal, "Strength");
						me.ShaderProperty(_WaveScaleGlobal, "Scale");
						me.ShaderProperty(_WaveSpeedGlobal, "Speed");
					});
					MGUI.BoldLabel("Wave 1");
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_WaveStrength0, "Strength");
						me.ShaderProperty(_WaveScale0, "Scale");
						me.ShaderProperty(_WaveSpeed0, "Speed");
						me.ShaderProperty(_WaveDirection0, "Direction");
					});
					MGUI.BoldLabel("Wave 2");
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_WaveStrength1, "Strength");
						me.ShaderProperty(_WaveScale1, "Scale");
						me.ShaderProperty(_WaveSpeed1, "Speed");
						me.ShaderProperty(_WaveDirection1, "Direction");
					});
					MGUI.BoldLabel("Wave 3");
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_WaveStrength2, "Strength");
						me.ShaderProperty(_WaveScale2, "Scale");
						me.ShaderProperty(_WaveSpeed2, "Speed");
						me.ShaderProperty(_WaveDirection2, "Direction");
					});
					MGUI.BoldLabel("Turbulence");
					MGUI.PropertyGroup(() => {
						me.ShaderProperty(_Turbulence, Tips.turbulence);
						me.ShaderProperty(_TurbulenceSpeed, "Speed");
						me.ShaderProperty(_TurbulenceScale, "Scale");
					});
				}
				MGUI.ToggleGroupEnd();
			};
			Foldouts.Foldout("VERTEX OFFSET", foldouts, vertTabButtons, mat, me, vertTabAction);

			// Caustics
			causticsTabButtons.Add(()=>{ResetCaustics();}, MGUI.resetLabel);
			Action causticsTabAction = ()=>{
				me.ShaderProperty(_CausticsToggle, "Enable");
				MGUI.ToggleGroup(_CausticsToggle.floatValue == 0);
				MGUI.Space4();
				MGUI.PropertyGroup( () => {
					me.ShaderProperty(_CausticsOpacity, "Strength");
					me.ShaderProperty(_CausticsDisp, "Phase");
					me.ShaderProperty(_CausticsSpeed, "Speed");
					me.ShaderProperty(_CausticsScale, "Scale");
					me.ShaderProperty(_CausticsFade, Tips.causticsFade);
					MGUI.Vector3Field(_CausticsRotation, "Rotation", false);
				});
				MGUI.PropertyGroup( ()=>{
					me.ShaderProperty(_CausticsDistortion, "Distortion Strength");
					me.ShaderProperty(_CausticsDistortionScale, "Distortion Scale");
					MGUI.Vector2Field(_CausticsDistortionSpeed, "Distortion Speed");
				});
				MGUI.ToggleGroupEnd();
			};
			Foldouts.Foldout("CAUSTICS", foldouts, causticsTabButtons, mat, me, causticsTabAction);
			
			// Foam
			foamTabButtons.Add(()=>{ResetFoam();}, MGUI.resetLabel);
			Action foamTabAction = ()=>{
				me.ShaderProperty(_FoamToggle, "Enable");
				MGUI.Space4();
				MGUI.ToggleGroup(_FoamToggle.floatValue == 0);
				MGUI.PropertyGroup( () => {
					me.TexturePropertySingleLine(foamLabel, _FoamTex, _FoamColor, _FoamStochasticToggle);
					MGUI.TexPropLabel(Tips.stochasticLabel, 172);
					MGUI.Space2();
					MGUI.Vector2Field(_FoamTexScale, "Scale");
					MGUI.Vector2Field(_FoamTexScroll, "Scrolling");
					me.ShaderProperty(_FoamOffset, Tips.parallaxOffsetLabel);
					me.ShaderProperty(_FoamDistortionStrength, "Distortion Strength");
					MGUI.ToggleFloat(me, Tips.foamNormal, _FoamNormalToggle, _FoamNormalStrength);
				});
				MGUI.PropertyGroup( () => {
					me.TexturePropertySingleLine(noiseLabel, _FoamNoiseTex);
					MGUI.Vector2Field(_FoamNoiseTexScale, "Scale");
					MGUI.Vector2Field(_FoamNoiseTexScroll, "Scrolling");
					me.ShaderProperty(_FoamNoiseTexStrength, Tips.foamNoiseTexStrength);
					me.ShaderProperty(_FoamNoiseTexCrestStrength, Tips.foamNoiseTexCrestStrength);
				});
				MGUI.PropertyGroup( () => {
					me.ShaderProperty(_FoamRoughness, Tips.foamRoughness);
					me.ShaderProperty(_FoamPower, Tips.foamPower);
					me.ShaderProperty(_FoamOpacity, Tips.foamOpacity);
					me.ShaderProperty(_FoamCrestStrength, Tips.foamCrestStrength);
					me.ShaderProperty(_FoamCrestThreshold, Tips.foamCrestThreshold);
				});
				MGUI.ToggleGroupEnd();
			};
			Foldouts.Foldout("FOAM", foldouts, foamTabButtons, mat, me, foamTabAction);

			// Depth Fog
			fogTabButtons.Add(()=>{ResetFog();}, MGUI.resetLabel);
			Action fogTabAction = ()=>{
				me.ShaderProperty(_FogToggle, "Enable");
				MGUI.Space4();
				MGUI.PropertyGroup( () => {
					MGUI.ToggleGroup(_FogToggle.floatValue == 0);
					me.ShaderProperty(_FogTint, "Color");
					me.ShaderProperty(_FogPower, "Power");
					MGUI.ToggleGroupEnd();
				});
			};
			Foldouts.Foldout("DEPTH FOG", foldouts, fogTabButtons, mat, me, fogTabAction);

			// Edge Fade
			edgeFadeTabButtons.Add(()=>{ResetEdgeFade();}, MGUI.resetLabel);
			Action edgeFadeTabAction = ()=>{
				me.ShaderProperty(_EdgeFadeToggle, "Enable");
				MGUI.Space4();
				MGUI.PropertyGroup( () => {
					MGUI.ToggleGroup(_EdgeFadeToggle.floatValue == 0);
					me.ShaderProperty(_EdgeFadePower, "Power");
					me.ShaderProperty(_EdgeFadeOffset, "Offset");
					MGUI.ToggleGroupEnd();
				});
			};
			Foldouts.Foldout("EDGE FADE", foldouts, edgeFadeTabButtons, mat, me, edgeFadeTabAction);

			// Rain
			rainTabButtons.Add(()=>{ResetRain();}, MGUI.resetLabel);
			Action rainTabAction = ()=>{
				me.ShaderProperty(_RainToggle, "Enable");
				MGUI.Space4();
				MGUI.PropertyGroup( () =>{
					MGUI.ToggleGroup(_RainToggle.floatValue == 0);
					me.ShaderProperty(_RippleStr, "Strength");
					me.ShaderProperty(_RippleSpeed, "Speed");
					me.ShaderProperty(_RippleScale, "Scale");
					MGUI.ToggleGroupEnd();
				});
			};
			Foldouts.Foldout("RAIN", foldouts, rainTabButtons, mat, me, rainTabAction);
        }
		ApplyMaterialSettings(mat);

		GUILayout.Space(20);
		float buttonSize = 35f;
		Rect footerRect = EditorGUILayout.GetControlRect();
		footerRect.x += (MGUI.GetInspectorWidth()/2f)-buttonSize-5f;
		footerRect.width = buttonSize;
		footerRect.height = buttonSize;
		if (GUI.Button(footerRect, MGUI.patIconTex))
			Application.OpenURL("https://www.patreon.com/mochieshaders");
		footerRect.x += buttonSize + 5f;
		footerRect.y += 17f;
		GUIStyle formatting = new GUIStyle();
		formatting.fontSize = 15;
		formatting.fontStyle = FontStyle.Bold;
		if (EditorGUIUtility.isProSkin){
			formatting.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 1);
			formatting.hover.textColor = new Color(0.8f, 0.8f, 0.8f, 1);
			GUI.Label(footerRect, versionLabel, formatting);
			footerRect.y += 20f;
			footerRect.x -= 35f;
			footerRect.width = 70f;
			footerRect.height = 70f;
			GUI.Label(footerRect, MGUI.mochieLogoPro);
			GUILayout.Space(90);
		}
		else {
			GUI.Label(footerRect, versionLabel, formatting);
			footerRect.y += 20f;
			footerRect.x -= 35f;
			footerRect.width = 70f;
			footerRect.height = 70f;
			GUI.Label(footerRect, MGUI.mochieLogo);
			GUILayout.Space(90);
		}
    }

	public override void AssignNewShaderToMaterial(Material mat, Shader oldShader, Shader newShader) {
		base.AssignNewShaderToMaterial(mat, oldShader, newShader);
		MGUI.ClearKeywords(mat);
	}

	void ApplyMaterialSettings(Material mat){
		bool ssrToggle = mat.GetInt("_SSR") == 1;
		int vertMode = mat.GetInt("_VertOffsetMode");
		int reflMode = mat.GetInt("_Reflections");
		int specMode = mat.GetInt("_Specular");
		int foamToggle = mat.GetInt("_FoamToggle");
		int foamNormalToggle = mat.GetInt("_FoamNormalToggle");
		bool foamNormals = foamToggle == 1 && foamNormalToggle == 1;

		MGUI.SetKeyword(mat, "_REFLECTIONS_ON", reflMode > 0);
		MGUI.SetKeyword(mat, "_REFLECTIONS_MANUAL_ON", reflMode == 2);
		MGUI.SetKeyword(mat, "_SPECULAR_ON", specMode > 0);
		MGUI.SetKeyword(mat, "_SCREENSPACE_REFLECTIONS_ON", ssrToggle);
		MGUI.SetKeyword(mat, "_VERTEX_OFFSET_ON", vertMode == 1);
		MGUI.SetKeyword(mat, "_GERSTNER_WAVES_ON", vertMode == 2);
		MGUI.SetKeyword(mat, "_FOAM_NORMALS_ON", foamNormals);
	}
	
	void ResetSurface(){
		_Color.colorValue = Color.white;
		_MainTex.textureValue = null;
		_MainTexScroll.vectorValue = new Vector4(0,0.1f,0,0);
		_DistortionStrength.floatValue = 0.1f;
		_Opacity.floatValue = 1f;
		_CullMode.floatValue = 2f;
		_BaseColorStochasticToggle.floatValue = 0f;
		_BaseColorOffset.floatValue = 0f;
		_ZWrite.floatValue = 0f;
		_BaseColorDistortionStrength.floatValue = 0.1f;
	}

	void ResetPrimaryNormal(){
		_NormalMapScale0.vectorValue = new Vector4(3f,3f,0,0);
		_NormalStr0.floatValue = 0.1f;
		_Rotation0.floatValue = 0f;
		_NormalMapScroll0.vectorValue = new Vector4(0.1f,0.1f,0,0);
		_Normal0StochasticToggle.floatValue = 0f;
		_NormalMapOffset0.floatValue = 0f;
	}

	void ResetSecondaryNormal(){
		_NormalStr1.floatValue = 0.2f;
		_NormalMapScale1.vectorValue = new Vector4(4f,4f,0,0);
		_NormalMapScroll1.vectorValue = new Vector4(-0.1f, 0.1f, 0,0);
		_Rotation1.floatValue = 0f;
		_Normal1StochasticToggle.floatValue = 0f;
		_NormalMapOffset1.floatValue = 0f;
	}
	
	void ResetFlowMapping(){
		_FlowSpeed.floatValue = 0.25f;
		_FlowStrength.floatValue = 0.1f;
		_FlowMapScale.vectorValue = new Vector4(2f,2f,0,0);
	}

	void ResetVertOffset(){
		_WaveScaleGlobal.floatValue = 1f;
		_WaveSpeedGlobal.floatValue = 1f;
		_WaveStrengthGlobal.floatValue = 1f;
		_NoiseTexScale.vectorValue = new Vector4(1,1,0,0);
		_NoiseTexScroll.vectorValue = new Vector4(0.3f,0.06f,0,0);
		_NoiseTexBlur.floatValue = 0.8f;
		_WaveHeight.floatValue = 0.1f;
		_Offset.vectorValue = new Vector4(0,1,0,0);
		_WaveSpeed0.floatValue = 1f;
		_WaveSpeed1.floatValue = 1.1f;
		_WaveSpeed2.floatValue = 1.2f;
		_WaveStrength0.floatValue = 0.1f;
		_WaveStrength1.floatValue = 0.1f;
		_WaveStrength2.floatValue = 0.1f;
		_WaveScale0.floatValue = 4f;
		_WaveScale1.floatValue = 2f;
		_WaveScale2.floatValue = 1f;
		_WaveDirection0.floatValue = 0f;
		_WaveDirection1.floatValue = 0f;
		_WaveDirection2.floatValue = 0f;
		_TurbulenceSpeed.floatValue = 0.3f;
		_Turbulence.floatValue = 1f;
		_TurbulenceScale.floatValue = 3f;
		_VertRemapMin.floatValue = -1f;
		_VertRemapMax.floatValue = 1f;
	}

	void ResetCaustics(){
		_CausticsOpacity.floatValue = 1f;
		_CausticsScale.floatValue = 15f;
		_CausticsSpeed.floatValue = 3f;
		_CausticsFade.floatValue = 5f;
		_CausticsDistortion.floatValue = 0.1f;
		_CausticsDisp.floatValue = 0.25f;
		_CausticsDistortionSpeed.vectorValue = new Vector4(-0.1f, -0.1f, 0f, 0f);
		_CausticsDistortionScale.floatValue = 1f;
		_CausticsRotation.vectorValue = new Vector4(-20f,0,-20f,0);
	}

	void ResetFog(){
		_FogTint.colorValue = new Vector4(0.11f,0.26f,0.26f,1f);
		_FogPower.floatValue = 100f;
	}

	void ResetFoam(){
		_FoamTexScale.vectorValue = new Vector4(3,3,0,0);
		_FoamRoughness.floatValue = 0.6f;
		_FoamColor.colorValue = Color.white;
		_FoamPower.floatValue = 200f;
		_FoamOpacity.floatValue = 3f;
		_FoamTexScroll.vectorValue = new Vector4(0.1f,-0.1f,0,0);
		_FoamStochasticToggle.floatValue = 0f;
		_FoamOffset.floatValue = 0f;
		_FoamCrestStrength.floatValue = 20f;
		_FoamCrestThreshold.floatValue = 0.5f;
		_FoamNoiseTexScroll.vectorValue = new Vector4(0f,0.1f,0f,0f);
		_FoamNoiseTexStrength.floatValue = 0f;
		_FoamNoiseTexCrestStrength.floatValue = 1.1f;
		_FoamNoiseTexScale.vectorValue = new Vector4(2f,2f,0,0);
		_FoamDistortionStrength.floatValue = 0.1f;
		_FoamNormalStrength.floatValue = 4f;
		_FoamNormalToggle.floatValue = 1f;
	}

	void ResetReflSpec(){
		_Roughness.floatValue = 0f;
		_Metallic.floatValue = 0f;
		_Reflections.floatValue = 1f;
		_ReflStrength.floatValue = 1f;
		_Specular.floatValue = 1f;
		_SpecStrength.floatValue = 1f;
		_SSR.floatValue = 0f;
		_SSRStrength.floatValue = 1f;
		_ReflTint.colorValue = Color.white;
		_SpecTint.colorValue = Color.white;
		_ReflCube.textureValue = null;
		_LightDir.vectorValue = new Vector4(0f,0.75f,1f,0f);
		_ReflCubeRotation.vectorValue = Vector4.zero;
	}

	void ResetRain(){
		_RippleScale.floatValue = 40f;
		_RippleSpeed.floatValue = 10f;
		_RippleStr.floatValue = 1f;
	}

	void ResetEdgeFade(){
		_EdgeFadePower.floatValue = 200f;
		_EdgeFadeOffset.floatValue = 0.5f;
	}

	void ClearDictionaries(){
		baseTabButtons.Clear();
		norm0TabButtons.Clear();
		norm1TabButtons.Clear();
		flowTabButtons.Clear();
		vertTabButtons.Clear();
		causticsTabButtons.Clear();
		fogTabButtons.Clear();
		foamTabButtons.Clear();
		edgeFadeTabButtons.Clear();
		reflSpecTabButtons.Clear();
		rainTabButtons.Clear();
	}
}