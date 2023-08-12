﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Aki.Reflection.Utils;
using UnityEngine;
using BindableState = BindableState<Diz.DependencyManager.ELoadState>;

namespace Aki.Custom.Utils
{
    public class EasyBundleHelper
    {
        private const BindingFlags _NonPublicInstanceflags = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo _pathField;
        private static readonly FieldInfo _keyWithoutExtensionField;
        private static readonly FieldInfo _bundleLockField;
        private static readonly PropertyInfo _dependencyKeysProperty;
        private static readonly PropertyInfo _keyProperty;
        private static readonly PropertyInfo _loadStateProperty;
        private static readonly MethodInfo _loadingCoroutineMethod;
        private readonly object _instance;
        public static readonly Type Type;

        static EasyBundleHelper()
        {
            _ = nameof(IBundleLock.IsLocked);
            _ = nameof(BindableState.Bind);

            // Class can be found as a private array inside EasyAssets.cs, next to DependencyGraph<IEasyBundle> 
            Type = PatchConstants.EftTypes.Single(x => x.GetMethod("set_SameNameAsset", _NonPublicInstanceflags) != null);
            
            _pathField = Type.GetField("string_1", _NonPublicInstanceflags);
            _keyWithoutExtensionField = Type.GetField("string_0", _NonPublicInstanceflags);
            _bundleLockField = Type.GetFields(_NonPublicInstanceflags).FirstOrDefault(x => x.FieldType == typeof(IBundleLock));
            _dependencyKeysProperty = Type.GetProperty("DependencyKeys");
            _keyProperty = Type.GetProperty("Key");
            _loadStateProperty = Type.GetProperty("LoadState");
           
            // Function with 0 params and returns task (usually method_0())
            var possibleMethods = Type.GetMethods(_NonPublicInstanceflags).Where(x => x.GetParameters().Length == 0 && x.ReturnType == typeof(Task));
            if (possibleMethods.Count() > 1)
            {
                Console.WriteLine($"Unable to find desired method as there are multiple possible matches: {string.Join(",", possibleMethods.Select(x => x.Name))}");
            }

            _loadingCoroutineMethod = possibleMethods.SingleOrDefault();
        }

        public EasyBundleHelper(object easyBundle)
        {
            _instance = easyBundle;
        }

        public IEnumerable<string> DependencyKeys
        {
            get
            {
                return (IEnumerable<string>)_dependencyKeysProperty.GetValue(_instance);
            }
            set
            {
                _dependencyKeysProperty.SetValue(_instance, value);
            }
        }

        public IBundleLock BundleLock
        {
            get
            {
                return (IBundleLock)_bundleLockField.GetValue(_instance);
            }
            set
            {
                _bundleLockField.SetValue(_instance, value);
            }
        }

        public string Path
        {
            get
            {
                return (string)_pathField.GetValue(_instance);
            }
            set
            {
                _pathField.SetValue(_instance, value);
            }
        }

        public string Key
        {
            get
            {
                return (string)_keyProperty.GetValue(_instance);
            }
            set
            {
                _keyProperty.SetValue(_instance, value);
            }
        }

        public BindableState LoadState
        {
            get
            {
                return (BindableState)_loadStateProperty.GetValue(_instance);
            }
            set
            {
                _loadStateProperty.SetValue(_instance, value);
            }
        }

        public string KeyWithoutExtension
        {
            get
            {
                return (string)_keyWithoutExtensionField.GetValue(_instance);
            }
            set
            {
                _keyWithoutExtensionField.SetValue(_instance, value);
            }
        }

        public Task LoadingCoroutine(Dictionary<string, AssetBundle> bundles)
        {
            return (Task)_loadingCoroutineMethod.Invoke(_instance, new object[] { bundles });
        }
    }
}
