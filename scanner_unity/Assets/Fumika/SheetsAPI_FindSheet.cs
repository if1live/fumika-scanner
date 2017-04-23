using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Fumika {
    class SheetsAPI_FindSheet {
        readonly GoogleDrive drive;
        readonly MonoBehaviour behaviour;

        public string SheetID { get; private set; }
        public bool Found { get; private set; }

        public SheetsAPI_FindSheet(GoogleDrive drive, MonoBehaviour behaviour) {
            this.drive = drive;
            this.behaviour = behaviour;
        }

        public IEnumerator BeginFindSheet(string sheetName) {
            Found = false;

            var query = string.Format("title = '{0}'", sheetName);
            var list = drive.ListFilesByQueary(query);
            yield return behaviour.StartCoroutine(list);
            var files = GoogleDrive.GetResult<List<GoogleDrive.File>>(list);

            if (files.Count == 0) {
                yield break;
            }

            var file = files[0];
            SheetID = file.ID;
            Found = true;
        }
    }
}
