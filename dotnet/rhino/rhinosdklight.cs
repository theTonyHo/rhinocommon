#pragma warning disable 1591
using System;
using System.Collections.Generic;
using Rhino.Geometry;

#if RHINO_SDK
namespace Rhino.DocObjects
{
  public class LightObject : RhinoObject
  {
    internal LightObject(uint serialNumber)
      : base(serialNumber) { }

    public Light LightGeometry
    {
      get
      {
        Light rc = Geometry as Light;
        return rc;
      }
    }
    public Light DuplicateLightGeometry()
    {
      Light rc = DuplicateGeometry() as Light;
      return rc;
    }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoLight_InternalCommitChanges;
    }
  }
}

namespace Rhino.DocObjects.Tables
{
  public class LightTable : IEnumerable<LightObject>, Rhino.Collections.IRhinoTable<LightObject>
  {
    private readonly RhinoDoc m_doc;
    internal LightTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Document that owns this light table.</summary>
    public RhinoDoc Document
    {
      get { return m_doc; }
    }

#if RDK_CHECKED
    private Rhino.Render.Sun m_sun;
    /// <summary>
    /// Gets the Sun instance that is applied to the document.
    /// <para>If the RDK is loaded, an instance is always returned.</para>
    /// </summary>
    /// <exception cref="Rhino.Runtime.RdkNotLoadedException">If the RDK is not loaded.</exception>
    public Rhino.Render.Sun Sun
    {
      get
      {
        if (null == m_sun)
        {
          Rhino.Runtime.HostUtils.CheckForRdk(true, true);
          m_sun = new Rhino.Render.Sun(m_doc);
        }
        return m_sun;
      }
    }
#endif

#if RDK_UNCHECKED
    private Rhino.Render.Skylight m_skylight;
    public Rhino.Render.Skylight Skylight
    {
      get { return m_skylight ?? (m_skylight = new Rhino.Render.Skylight(m_doc)); }
    }
#endif

    /// <summary>Number of lights in the light table.  Does not include Sun or Skylight.</summary>
    public int Count
    {
      get
      {
        return UnsafeNativeMethods.CRhinoLightTable_LightCount(m_doc.m_docId);
      }
    }

    public Rhino.DocObjects.LightObject this[int index]
    {
      get
      {
        uint sn = UnsafeNativeMethods.CRhinoLightTable_Light(m_doc.m_docId, index);
        if( sn<1 )
          return null;
        return new LightObject(sn);
      }
    }

    ///// <summary>
    ///// Finds all of the lights with a given name
    ///// </summary>
    //public int[] FindByName(string lightName, bool ignoreDeletedLights)
    //{
    //}

    public int Find(Guid id, bool ignoreDeleted)
    {
      return UnsafeNativeMethods.CRhinoLightTable_Find(m_doc.m_docId, id, ignoreDeleted);
    }

    public int Add(Geometry.Light light)
    {
      return Add(light, null);
    }
    public int Add(Geometry.Light light, ObjectAttributes attributes)
    {
      IntPtr pConstLight = light.ConstPointer();
      IntPtr pConstAttributes = IntPtr.Zero;
      if (attributes != null) pConstAttributes = attributes.ConstPointer();
      return UnsafeNativeMethods.CRhinoLightTable_Add(m_doc.m_docId, pConstLight, pConstAttributes);
    }

    public bool Modify(Guid id, Geometry.Light light)
    {
      int index = Find(id, true);
      return Modify(index, light);
    }

    public bool Modify(int index, Geometry.Light light)
    {
      bool rc = false;
      if (index >= 0)
      {
        IntPtr pConstLight = light.ConstPointer();
        rc = UnsafeNativeMethods.CRhinoLightTable_Modify(m_doc.m_docId, index, pConstLight);
      }
      return rc;
    }

#region enumerator

    // for IEnumerable<Layer>
    public IEnumerator<LightObject> GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<LightTable, LightObject>(this);
    }

    // for IEnumerable
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<LightTable, LightObject>(this);
    }

    #endregion

  }
}
#endif