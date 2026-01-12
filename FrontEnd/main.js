const apiUrl = 'http://localhost:5169/api';

document.getElementById('ingredient-form').addEventListener('submit', async (e) => {
    e.preventDefault();

    const name = document.getElementById('ingredient-name').value;
    const type = document.getElementById('ingredient-type').value;
    const calories = document.getElementById('ingredient-calories').value;

    if(type=="null" || name==null || calories == null)
    {
        alert("Sva polja su obavezna!");
        return;
    }

    try{
        const response = await fetch(`${apiUrl}/ingredients/create`, {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                name:name, type:type, calories:parseInt(calories)
            })
        });

        console.log("Response status:", response.status);

        if(response.ok)
        {
            const data = await response.json();
            console.log("Response data:", data);

            window.location.href = "app.html";
        }
        else
        {
            throw new Error('Failed to create ingredient.');
        }
    }
    catch(error){
        console.error('Error:', error);
        alert(error.message);
    }
})

async function CreateRecipe(){
    const selectForm = document.getElementById("ingredient-options");

    try{
        const ingredientResponse = await fetch(`${apiUrl}/ingredients/names`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        });
        if (!ingredientResponse.ok) {
            throw new Error(`HTTP error! status: ${ingredientResponse.status}`);
        }
        const ingredients = await ingredientResponse.json();

        ingredients.forEach(ingredient => {
            const option = document.createElement("option");
            option.value = ingredient;
            option.text = ingredient;
            selectForm.appendChild(option);
        });

        document.getElementById('recipe-form').addEventListener('submit', async (e) => {
            e.preventDefault();
            const titleText = document.getElementById("recipe-title-text").value.trim();
            const prepText = document.getElementById("recipe-prep-text").value.trim();
            const descriptionText = document.getElementById("recipe-description-text").value.trim();
            const authorId = localStorage.getItem("token");

            // Prikupljanje izabranih sastojaka
            const selectedOptions = Array.from(selectForm.selectedOptions).map(option => option.value);

            if (selectedOptions.length === 0) {
                alert("Morate selektovati makar jedan sastojak!");
                return;
            }

            // Formiranje query stringa
            const queryParams = selectedOptions.map(name => `ingredientNames=${encodeURIComponent(name)}`).join("&");

            const recipeResponse = await fetch(`${apiUrl}/recipe/create/${authorId}?${queryParams}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    title: titleText,
                    prepTime: prepText,
                    description: descriptionText,
                }),
            });

            if (!recipeResponse.ok) {
                const errorText = await recipeResponse.text();
                console.error("Error response body:", errorText);
                throw new Error(`HTTP error! status: ${recipeResponse.status}, body: ${errorText}`);
            }

            const createdRecipe = await recipeResponse.json();
            console.log("Recipe created successfully:", createdRecipe);
            alert("Recept je uspesno kreiran!");
            window.location.href = "app.html"

        });
    }
    catch(error){
        console.error('Error during ingredients fetch', error);
        alert('Something went wrong');
    }

}

document.getElementById("new-ing-from-recipe").addEventListener('click', () => {
    const ingredientDiv = document.getElementById("ingredient-create-div");
    ingredientDiv.removeAttribute("hidden");
});

document.getElementById("create-recipe-unhidden").addEventListener('click', () => {
    const ingredientDiv = document.getElementById("recipe-form-div");
    ingredientDiv.removeAttribute("hidden");

    const button = document.getElementById("create-recipe-unhidden");
    button.hidden = true;
});

document.getElementById("logout-button").addEventListener('click', () => {
    window.location.href = "index.html"
    localStorage.removeItem("token");
});


async function fetchRecipes() {
    try {
        const response = await fetch(`${apiUrl}/recipe/returnAll`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (response.ok) {
            const recipes = await response.json();
            renderRecipes(recipes, "recipes-list");
        } else {
            alert('Failed to fetch friend posts');
        }
    } catch (error) {
        console.error('Error fetching friend posts:', error);
    }
}

function renderRecipes(data, divId){
    const divRecipes = document.getElementById(divId);
    divRecipes.innerHTML = "";

    data.forEach(item => {
        const divItem = document.createElement("div");
        divItem.className = "div-item"

        const divTitle = document.createElement("div");
        divTitle.className = "div-title-recipe"
        divTitle.innerHTML = item.title;
        divItem.appendChild(divTitle);

        const divContent = document.createElement("div");
        divContent.className = "div-content"
        divItem.appendChild(divContent)

        const labelIme = document.createElement("label");
        labelIme.innerHTML = "Ime autora: "+item.recipeAuthor.name;
        divContent.appendChild(labelIme);

        const labelTime = document.createElement("label");
        labelTime.innerHTML = "Vreme pripreme: "+item.prepTime+" min";
        divContent.appendChild(labelTime);

        const labelIngredients = document.createElement("label");
        labelIngredients.innerHTML = "Sastojci:";
        divContent.appendChild(labelIngredients);

        const divIngredients = document.createElement("div");
        divIngredients.className = "div-ingredients";
        divContent.appendChild(divIngredients);

        item.ingredients.forEach(ingredient =>{
            const labelIngredientName = document.createElement("label");
            labelIngredientName.innerHTML ="* "+ingredient.name;
            divIngredients.appendChild(labelIngredientName);
        })

        const labelCalories = document.createElement("label");
        labelCalories.innerHTML = "Ukupno kalorija po sastojku na 100g: " + item.totalCalories;
        divContent.appendChild(labelCalories);
        
        divRecipes.appendChild(divItem);

        const descriptionDiv = document.createElement("div");
        const labelDescription = document.createElement("label");
        labelDescription.innerHTML = "Opis: " + item.description;
        descriptionDiv.appendChild(labelDescription);
        divContent.appendChild(descriptionDiv);

        const updateForm = document.createElement("div");
        updateForm.style.display = "none";

        const updateInput = document.createElement("textarea");
        // updateInput.type = "text";
        updateInput.value = item.description;
        updateInput.className="update-input"
        updateInput.style.height = "auto";
        updateForm.appendChild(updateInput);

        const saveButton = document.createElement("button");
        saveButton.innerHTML = "Sačuvaj";
        saveButton.onclick = async function () {

            const updatedDescripion = updateInput.value;

            try{
                const response = await fetch(`${apiUrl}/recipe/updateDescription` , {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        id: item.id,
                        description: updatedDescripion
                    })
                });
        
                if (response.ok) {
                    const recipe = await response.json();
                    console.log(recipe);
                } else {
                    alert('Failed to fetch recipe');
                }
            }
            catch(error){
                console.error('Error:', error);
                alert(error.message);
            }            

            descriptionDiv.style.display = "block";
            updateForm.style.display = "none";
            window.location.href = "app.html";
        };
        updateForm.appendChild(saveButton);
        divContent.appendChild(updateForm);

        const updateButton = document.createElement("button");
        updateButton.innerHTML = "Ažuriraj opis";
        updateButton.className = "update-recipe-button";
        updateButton.onclick = function () {
            const authorId = localStorage.getItem("token");
            if(item.recipeAuthor.id != authorId)
            {
                alert("Možete ažurirati samo svoj recept!");
                return;
            }
            descriptionDiv.style.display = "none";
            updateForm.style.display = "block";
        };
        divContent.appendChild(updateButton);

        const deleteButton = document.createElement("button");
        deleteButton.innerHTML = "Obriši";
        deleteButton.className = "delete-recipe-button";
        deleteButton.onclick = async function () {
            const authorId = localStorage.getItem("token");
            if(item.recipeAuthor.id != authorId)
            {
                alert("Ne možete obrisati recept koji niste Vi kreirali!");
                return;
            }
        
            try{
                const response = await fetch(`${apiUrl}/recipe/deletePost/${item.id}` , {
                    method: 'DELETE',
                    headers: {
                        'Content-Type': 'application/json',
                    }
                });
        
                if (response.ok) {
                    alert("Uspešno ste obrisali recept!");
                    fetchRecipes();
                } else {
                    alert('Greška prilikom brisanja recepta!');
                }
            }
            catch(error){
                console.error('Error:', error);
                alert(error.message);
            }  
        }
        divContent.appendChild(deleteButton);
    })
}

function generateCheckboxes(groupId, values) {
    const container = document.getElementById(groupId);
    values.forEach(value => {
        const cblabelDiv = document.createElement("div");
        cblabelDiv.className = "cb-label-div";

        const checkbox = document.createElement("input");
        checkbox.type = "checkbox";
        checkbox.value = value;
        checkbox.name = groupId;
        checkbox.className = "filter-checkbox";
        
        const label = document.createElement("label");
        label.textContent = value;

        cblabelDiv.appendChild(checkbox);
        cblabelDiv.appendChild(label);
        container.appendChild(cblabelDiv);
    });
}

async function fetchUpdateDescription(recipeId, updatedDescription) {
    try{
        const response = await fetch(`${apiUrl}/recipe/updateDescription` , {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                id: recipeId,
                description: updatedDescription
            })
        });

        if (response.ok) {
            const recipe = await response.json();
            generateCheckboxes("cb-author-div", authors);
        } else {
            alert('Failed to fetch authors');
        }
    }
    catch(error){
        console.error('Error:', error);
        alert(error.message);
    }
}

async function fetchFilterAuthors() {
    try{
        const response = await fetch(`${apiUrl}/author/allNames` , {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (response.ok) {
            const authors = await response.json();
            generateCheckboxes("cb-author-div", authors);
        } else {
            alert('Failed to fetch authors');
        }
    }
    catch(error){
        console.error('Error:', error);
        alert(error.message);
    }
}

async function fetchFilterIngredients() {
    try{
        const response = await fetch(`${apiUrl}/ingredients/names` , {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            }
        });

        if (response.ok) {
            const ingredients = await response.json();
            generateCheckboxes("cb-ingredients-div", ingredients);
        } else {
            alert('Failed to fetch ingredients');
        }
    }
    catch(error){
        console.error('Error:', error);
        alert(error.message);
    }
}

fetchFilterAuthors();
fetchFilterIngredients();

async function filterRecipes() {
    document.getElementById("button-filter").addEventListener("click", async () => {
        var checkedAuthors = document.querySelectorAll('input[name="cb-author-div"]:checked');
        var authorValues = Array.from(checkedAuthors).map(cb => cb.value);

        var checkedIngredients = document.querySelectorAll('input[name="cb-ingredients-div"]:checked');
        var ingredientsValues = Array.from(checkedIngredients).map(cb => cb.value);

        var minPrepTime = document.getElementById("minPrep").value;
        var maxPrepTime = document.getElementById("maxPrep").value;

        const queryAuthors = authorValues.map(name => `authorNames=${encodeURIComponent(name)}`).join("&");
        const queryIngredients = ingredientsValues.map(name => `ingredientsNames=${encodeURIComponent(name)}`).join("&");
        const queryPrepTimes = `prepTimes=${encodeURIComponent(minPrepTime)}&prepTimes=${encodeURIComponent(maxPrepTime)}`;

        try{
            const response = await fetch(`${apiUrl}/recipe/filter?${queryAuthors}&${queryIngredients}&${queryPrepTimes}` , {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                }
            });

            if(response.ok){
                const data = await response.json();
                renderRecipes(data, "recipes-list");
            }
    
        }
        catch(error){
            console.error('Error:', error);
            alert(error.message);
        }
    });
}

document.addEventListener("DOMContentLoaded", () => {
    const minPrep = document.getElementById("minPrep");
    const maxPrep = document.getElementById("maxPrep");
    const prepTimeRange = document.getElementById("prepTimeRange");

    function updatePrepTimeDisplay() {
        prepTimeRange.textContent = `${minPrep.value} min - ${maxPrep.value} min`;
    }

    minPrep.addEventListener("input", updatePrepTimeDisplay);
    maxPrep.addEventListener("input", updatePrepTimeDisplay);
});

CreateRecipe();
fetchRecipes();
filterRecipes();