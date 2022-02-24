// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/OverdrawFX"
{
   
        Properties{
            _MainTex("Base", 2D) = "white" {}
            _Color("Main Color", Color) = (0.15,0.0,0.0,0.0)
        }

            SubShader{
                Fog { Mode Off }
                ZWrite Off
                ZTest Always
                Blend One One // additive blending

                Pass {
                    SetTexture[_MainTex] {
                        constantColor[_Color]
                        combine constant, texture
                    }
                }
            }
    }
