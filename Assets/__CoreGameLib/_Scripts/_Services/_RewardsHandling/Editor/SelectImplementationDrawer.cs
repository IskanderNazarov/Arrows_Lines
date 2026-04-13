// Файл: Editor/SelectImplementationDrawer.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
[CustomPropertyDrawer(typeof(SelectImplementationAttribute))]
public class SelectImplementationDrawer : PropertyDrawer {

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        // Запрашиваем высоту всего дерева свойств (включая детей, если раскрыто)
        return EditorGUI.GetPropertyHeight(property, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        // 1. Вычисляем Rect для заголовка (первая строка)
        var headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

        // 2. Рисуем стандартную стрелочку Foldout и обрабатываем нажатие на неё
        // Важно: мы не рисуем label здесь, только стрелочку
        var foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none, true);

        // 3. Рисуем Кнопку выбора типа поверх того места, где обычно был бы Label
        // Смещаем кнопку вправо, чтобы не перекрывать стрелочку Foldout (она обычно занимает около 15px)
        var indent = EditorGUI.indentLevel * 15f;
        var buttonRect = new Rect(position.x + 15 + indent, position.y, position.width - 15 - indent, EditorGUIUtility.singleLineHeight);

        var typeName = GetTypeName(property);

        // Рисуем кнопку. Если нажали - открываем меню
        if (GUI.Button(buttonRect, new GUIContent(typeName, "Click to change reward type"), EditorStyles.miniButton)) {
            ShowTypeMenu(property);
        }

        // 4. Рисуем содержимое (поля класса), ТОЛЬКО если свойство раскрыто
        if (property.isExpanded) {
            // Используем PropertyField с includeChildren = true. 
            // Но так как мы уже нарисовали заголовок сами, нам нужно пропустить отрисовку заголовка
            // Unity не дает простого способа отрисовать "только детей", поэтому мы используем этот трюк:

            EditorGUI.indentLevel++;

            // Находим первое дочернее свойство
            var iterator = property.Copy();
            var endProperty = iterator.GetEndProperty();

            // Входим внутрь объекта
            if (iterator.NextVisible(true)) {
                // Рисуем все дочерние элементы, пока не дойдем до конца объекта
                while (!SerializedProperty.EqualContents(iterator, endProperty)) {
                    var childHeight = EditorGUI.GetPropertyHeight(iterator, true);
                    var childRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, childHeight); // +2 padding

                    // Смещаем Y позицию для следующего элемента
                    position.y += childHeight + 2;

                    EditorGUI.PropertyField(childRect, iterator, true);

                    if (!iterator.NextVisible(false)) break;
                }
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private string GetTypeName(SerializedProperty property) {
        if (string.IsNullOrEmpty(property.managedReferenceFullTypename))
            return "Select Type (Null)";

        var parts = property.managedReferenceFullTypename.Split(' ');
        if (parts.Length == 2) return parts[1].Split('.').Last();
        return "Unknown";
    }

    private void ShowTypeMenu(SerializedProperty property) {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Null"), false, () => {
            property.managedReferenceValue = null;
            property.serializedObject.ApplyModifiedProperties();
        });

        var fieldType = fieldInfo.FieldType;
        if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>)) {
            fieldType = fieldType.GetGenericArguments()[0];
        }
        else if (fieldType.IsArray) {
            fieldType = fieldType.GetElementType();
        }

        var types = TypeCache.GetTypesDerivedFrom(fieldType)
            .Where(p => !p.IsAbstract && !p.IsInterface).ToList();

        foreach (var type in types) {
            menu.AddItem(new GUIContent(type.Name), false, () => {
                // Создаем новый экземпляр
                property.managedReferenceValue = Activator.CreateInstance(type);

                // ВАЖНО: Сразу раскрываем свойство, чтобы юзер увидел поля
                property.isExpanded = false;

                property.serializedObject.ApplyModifiedProperties();
            });
        }
        menu.ShowAsContext();
    }
}
