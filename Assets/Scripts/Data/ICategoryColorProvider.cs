using UnityEngine;

public interface ICategoryColorProvider {
    Color GetColorForCategory(QuestionCategory category);
}

public class CategoryColorProvider : ICategoryColorProvider {
    private CategoryColorConfig categoryColorConfig;

    public CategoryColorProvider(CategoryColorConfig categoryColorConfig) {
        this.categoryColorConfig = categoryColorConfig;
    }

    public Color GetColorForCategory(QuestionCategory category) {
        if (categoryColorConfig.CategoryColors.TryGetValue(category, out var color)) {
            return color;
        }
        return categoryColorConfig.DefaultColor;
    }
}