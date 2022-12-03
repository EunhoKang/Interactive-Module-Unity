Shader "Unlit/sten"
{
    Properties{
        [IntRange] _StencilID("Stencil ID",Range(0,255)) = 0
    }

        SubShader{
            Tags{
                "Queue" = "Geometry-1"
            }
            Pass{
                Zwrite off
                ColorMask 0
                Cull front
        Stencil{
                Ref[_StencilID]
                Comp always
                Pass replace
            }
        }
    }
}
