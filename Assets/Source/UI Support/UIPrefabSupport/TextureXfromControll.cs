using UnityEngine;

public class TextureXfromControl : XfromControl
{
    // table cloth
    public TexturePlacement tableCloth;

    // store the last values for each mode (TSR)
    private Vector3 mTranslate = Vector3.zero;  // only X,Y used
    private Vector3 mScale     = Vector3.one;   // only X,Y used
    private Vector3 mRotate    = Vector3.zero;  // only Z used (degrees)

    new void Start()
    {
        T.onValueChanged.AddListener(SetToTranslation);
        R.onValueChanged.AddListener(SetToRotation);
        S.onValueChanged.AddListener(SetToScaling);

        X.SetSliderListener(XValueChanged);
        Y.SetSliderListener(YValueChanged);
        Z.SetSliderListener(ZValueChanged);

        T.isOn = false;
        R.isOn = true;
        S.isOn = false;
        SetToRotation(true);

        //if (tableCloth != null && ObjectName != null)
        //{
        //    ObjectName.text = "Texture: " + tableCloth.gameObject.name;
        //}
    }

    #region slide bars initialization
    // ----------------------------------------------------------------------
    // Initialize slider bars for specific function (T / S / R)
    void SetToTranslation(bool v)
    {
        if (!v || tableCloth == null)
        {
            return;
        }
        mTranslate.x = tableCloth.UV_Translate_X;
        mTranslate.y = tableCloth.UV_Translate_Y;
        mTranslate.z = 0f;

        X.InitSliderRange(-5f, 5f, mTranslate.x);
        Y.InitSliderRange(-5f, 5f, mTranslate.y);
        Z.InitSliderRange(-5f, 5f, mTranslate.z);
    }

    void SetToScaling(bool v)
    {
        if (!v || tableCloth == null)
        { 
            return; 
        }

        mScale.x = tableCloth.UV_Scale_X;
        mScale.y = tableCloth.UV_Scale_Y;
        mScale.z = 1f;

        X.InitSliderRange(0.1f, 5f, mScale.x);
        Y.InitSliderRange(0.1f, 5f, mScale.y);
        Z.InitSliderRange(0.1f, 5f, mScale.z);
    }

    void SetToRotation(bool v)
    {
        if (!v || tableCloth == null)
        {
            return;
        }

        mRotate.z = tableCloth.UV_Rotation;
        mRotate.x = 0f;
        mRotate.y = 0f;

        // X and Y not used. Value locked at 0f
        X.InitSliderRange(0f, 0f, 0f);
        Y.InitSliderRange(0f, 0f, 0f);
        Z.InitSliderRange(-180f, 180f, mRotate.z);
    }
    #endregion

    #region handle slider changes
    // ----------------------------------------------------------------------
    // Respond to slider changes
    void XValueChanged(float v)
    {
        if (tableCloth == null) return;

        if (T.isOn)
        {
            mTranslate.x = v;
            tableCloth.UV_Translate_X = v;
        }
        else if (S.isOn)
        {
            mScale.x = v;
            tableCloth.UV_Scale_X = v;
        }
        // R mode: X unused
    }

    void YValueChanged(float v)
    {
        if (tableCloth == null) return;

        if (T.isOn)
        {
            mTranslate.y = v;
            tableCloth.UV_Translate_Y = v;
        }
        else if (S.isOn)
        {
            mScale.y = v;
            tableCloth.UV_Scale_Y = v;
        }
        // R mode: Y unused
    }

    void ZValueChanged(float v)
    {
        if (tableCloth == null) return;

        if (R.isOn)
        {
            mRotate.z = v;
            tableCloth.UV_Rotation = v;
        }
        else if (S.isOn)
        {
            // optional: uniform scale on Z
            mScale.x = v;
            mScale.y = v;
            tableCloth.UV_Scale_X = v;
            tableCloth.UV_Scale_Y = v;
        }
        // T mode: Z unused
    }
    #endregion

    #region override parents function
    public new void SetSelectedObject(Transform xform)
    {
        return;
    }
    #endregion
}
