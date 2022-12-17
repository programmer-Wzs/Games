using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Wu
{
    public class CreateAB : EditorWindow
    {
        // ƽ̨
        string[] buildTargets = new string[] { "Windows", "IOS", "Android" };
        BuildTarget[] bTs = new BuildTarget[] { BuildTarget.StandaloneWindows, BuildTarget.iOS, BuildTarget.Android };
        int selectTgIndex = 0;

        // �Ƿ�������Դ�Ա��ļ�����Դ���� ��Դmd5��
        bool createList = false;
        static string listName = "abList.txt";

        // ���·��
        string outPath = "AssetBundles/Windows";

        // ѡ��
        BuildAssetBundleOptions buildABOp = BuildAssetBundleOptions.None;

        string[] cpKinds = new string[] { "None", "LZMA", "LZ4" };
        BuildAssetBundleOptions[] cps = new BuildAssetBundleOptions[] { BuildAssetBundleOptions.UncompressedAssetBundle, BuildAssetBundleOptions.None, BuildAssetBundleOptions.ChunkBasedCompression };
        int selectCpIndex = 1;

        // ��������ļ�������
        bool clearFolders = false;
        // ���Ƶ� Assets/StreamingAssets/
        bool copyToSA = false;

        bool excludeTI = false;
        bool forceRebuild = false;
        bool ignoreTTC = false;
        bool appendHash = false;
        bool strictMode = false;
        bool dryRunBuild = false;


        [MenuItem("ABTools/CreateAB")]
        private static void createAB()
        {
            //��������
            (EditorWindow.GetWindowWithRect(typeof(CreateAB), new Rect(10, 10, 220, 550)) as CreateAB).Show();


            // ����
            //BuildPipeline.BuildAssetBundles("Assets/AssetBundles",BuildAssetBundleOptions.None,BuildTarget.StandaloneWindows);


        }

        private void OnGUI()
        {
            // ƽ̨
            GUI.Label(new Rect(10, 10, 200, 20), "ƽ̨");
            selectTgIndex = GUI.Toolbar(new Rect(10, 30, 200, 20), selectTgIndex, buildTargets);

            // ���·��
            GUI.Label(new Rect(10, 70, 200, 20), "���·��");
            outPath = GUI.TextField(new Rect(10, 90, 200, 20), outPath);

            // ѹ����ʽ
            GUI.Label(new Rect(10, 130, 200, 20), "ѹ����ʽ");
            selectCpIndex = GUI.Toolbar(new Rect(10, 150, 200, 20), selectCpIndex, cpKinds);

            // ����ѡ��
            int y = 190;
            GUI.Label(new Rect(10, y, 200, 20), "����");
            y += 30;

            clearFolders = GUI.Toggle(new Rect(10, y, 200, 20), clearFolders, "--Clear Folders");
            y += 30;

            copyToSA = GUI.Toggle(new Rect(10, y, 200, 20), copyToSA, "--Copy to StreamingAssets");
            y += 30;

            excludeTI = GUI.Toggle(new Rect(10, y, 200, 20), excludeTI, "Exclude Type Information");
            y += 30;

            excludeTI = GUI.Toggle(new Rect(10, y, 200, 20), excludeTI, "Exclude Type Information");
            y += 30;
            forceRebuild = GUI.Toggle(new Rect(10, y, 200, 20), forceRebuild, "Force Rebuild");
            y += 30;
            ignoreTTC = GUI.Toggle(new Rect(10, y, 200, 20), ignoreTTC, "Ignore Type Tree Changes");
            y += 30;
            appendHash = GUI.Toggle(new Rect(10, y, 200, 20), appendHash, "Append Hash");
            y += 30;
            strictMode = GUI.Toggle(new Rect(10, y, 200, 20), strictMode, "Strict Mode");
            y += 30;
            dryRunBuild = GUI.Toggle(new Rect(10, y, 200, 20), dryRunBuild, "Dry Run Build");
            y += 30;

            createList = GUI.Toggle(new Rect(10, y, 200, 20), createList, "*������Դ�Ա��ļ�");
            y += 30;

            if (GUI.Button(new Rect(10, y, 200, 20), "Build"))
            {
                Build();
            }
        }

        // ����AB��
        private void Build()
        {
            AsyncBuild((isOk) => {
                if (isOk)
                {
                    // �������
                    Debug.Log("ok");
                }
                else
                {
                    // ����ʧ��
                    Debug.Log("error");
                }
            });
        }

        private async void AsyncBuild(Action<bool> bulidOkAction)
        {
            bool isOk = false;

            buildAB(); // ����AB��
            await Task.Run(() =>
            {
                if (createList)
                    CreateList(outPath); // ������Դ�Ա��ļ�
                if (copyToSA) CopyToStreamingAssets();
                isOk = true;
            });

            bulidOkAction(isOk);
            AssetDatabase.Refresh();
        }

        // ������Դ�Ա��ļ�: ����Ŀ¼ �� �ļ����˺���(���˲���Ҫ����md5���ļ�)
        public static bool CreateList(string dirPath, Func<string, bool> fileFilter = null)
        {
            if (!Directory.Exists(dirPath))
                return false;
            if (fileFilter == null) fileFilter = TestFileValid;

            string[] filenames = Directory.GetFiles(dirPath);

            string newListName = dirPath + "/" + listName;

            StringBuilder listText = new StringBuilder();

            foreach (string filename in filenames)
            {
                if (!fileFilter(filename)) continue;
                listText.Append(filename.Substring(filename.LastIndexOf("\\") + 1));
                listText.Append(" ");
                listText.Append(FileToMd5(filename));
                listText.Append("-");
            }
            if (listText.Length > 0) listText.Remove(listText.Length - 1, 1);

            try
            {
                File.WriteAllText(newListName, listText.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return false;
            }

            return true;
        }

        // ����MD5
        public static string FileToMd5(string filePath)
        {
            if (!File.Exists(filePath)) return "";

            using (FileStream file = new FileStream(filePath, FileMode.Open))
            {
                byte[] bytes = (new MD5CryptoServiceProvider()).ComputeHash(file);

                StringBuilder bytesToH = new StringBuilder();
                for (int i = 0; i < bytes.Length; ++i)
                {
                    bytesToH.Append(bytes[i].ToString("x2"));
                }
                return bytesToH.ToString();
            }
        }

        // �������� ����AB��
        private void buildAB()
        {
            // ��������
            buildABOp |= cps[selectCpIndex];
            if (excludeTI) buildABOp |= BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;
            if (forceRebuild) buildABOp |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            if (ignoreTTC) buildABOp |= BuildAssetBundleOptions.IgnoreTypeTreeChanges;
            if (appendHash) buildABOp |= BuildAssetBundleOptions.AppendHashToAssetBundleName;
            if (strictMode) buildABOp |= BuildAssetBundleOptions.StrictMode;
            if (dryRunBuild) buildABOp |= BuildAssetBundleOptions.DryRunBuild;

            if (!Directory.Exists(outPath)) Directory.CreateDirectory(outPath);
            else if (clearFolders) { Directory.Delete(outPath, true); Directory.CreateDirectory(outPath); }
            // ����
            BuildPipeline.BuildAssetBundles(outPath, buildABOp, bTs[selectTgIndex]);
        }

        // �ж��Ƿ�Ϊ��Ҫ��������Md5���ļ�
        private static bool TestFileValid(string fileName)
        {
            if (fileName.IndexOf(".") == -1)
                return true;
            return false;
        }

        private void CopyToStreamingAssets()
        {
            if (!Directory.Exists(outPath)) return;

            string path = "Assets/StreamingAssets";

            string[] filenames = Directory.GetFiles(outPath);


            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                else if (clearFolders) { Directory.Delete(path, true); Directory.CreateDirectory(path); }

                string toName;
                foreach (string filename in filenames)
                {
                    if (TestFileValid(filename) || filename.Substring(filename.LastIndexOf("\\") + 1).Equals(listName))
                    {
                        toName = path + "/" + filename.Substring(filename.LastIndexOf("\\") + 1);
                        if (File.Exists(toName)) File.Delete(toName);
                        File.Copy(filename, toName);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("ToSA error " + ex.Message);
                return;
            }
        }
    }

}

