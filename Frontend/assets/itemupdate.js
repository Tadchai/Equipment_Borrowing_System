const urlParams = new URLSearchParams(window.location.search);
const itemCategoryId = urlParams.get("id");

async function fetchItemDetail() {
  try {
    const response = await fetch(
      `http://localhost:5143/Item/GetById?id=${itemCategoryId}`
    );
    const data = await response.json();

    if (!response.ok || !data) {
      document.getElementById("itemDetail").innerHTML = `<p>Error: ${
        data.message || "Failed to load item details."
      }</p>`;
      return;
    }

    renderItemDetail(data);
  } catch (error) {
    console.error("Error fetching item detail:", error);
    document.getElementById(
      "itemDetail"
    ).innerHTML = `<p>An error occurred while fetching item details.</p>`;
  }
}

function renderItemDetail(data) {
  let html = `<h2>Item Category</h2>`;
  html += `<p>Name: <input type="text" id="categoryName" value="${data.name}"></p>`;

  html += `<h3>Item Classifications</h3>`;
  html += `<div id="classificationsContainer">`;
  data.itemClassifications.forEach((classification, index) => {
    html += renderClassification(classification, index);
  });
  html += `</div>`;

  document.getElementById("itemDetail").innerHTML = html;

  document
    .querySelectorAll(".add-instance-button")
    .forEach((button) => button.addEventListener("click", addInstanceRow));
  document
    .querySelectorAll(".delete-classification-button")
    .forEach((button) =>
      button.addEventListener("click", deleteClassification)
    );
  document
    .querySelectorAll(".delete-instance-button")
    .forEach((button) => button.addEventListener("click", deleteInstanceRow));
}

function renderClassification(classification, index) {
  let html = `<div class="classification-box" data-index="${index}" data-id="${
    classification.id || ""
  }">
        <p>
          Name: <input type="text" class="classification-name" value="${
            classification.name
          }">
          <button class="delete-classification-button" data-classification-index="${index}">Delete Classification</button>
        </p>
        <button class="add-instance-button" data-classification-index="${index}">Add Instance</button>
        <h4>Item Instances</h4>
        <div class="instances-container">`;

  classification.itemInstances.forEach((instance, instanceIndex) => {
    html += renderInstance(instance, instanceIndex);
  });

  html += `</div></div>`;
  return html;
}

function renderInstance(instance = { id: null, assetId: "" }, instanceIndex = -1) {
  const assetId = instance.assetId || ""; 
  return `
    <div class="instance-row" data-instance-index="${instanceIndex}" data-id="${instance.id || ""}">
      <input type="text" class="instance-asset-id" value="${assetId}">
      <button class="delete-instance-button" data-instance-index="${instanceIndex}">Delete Instance</button>
    </div>`;
}



function addClassification() {
  const container = document.getElementById("classificationsContainer");
  const index = container.children.length;

  const newClassification = `
      <div class="classification-box" data-index="${index}">
        <p>
          Name: <input type="text" class="classification-name" placeholder="New Classification Name">
          <button class="delete-classification-button" data-classification-index="${index}">Delete Classification</button>
        </p>
        <button class="add-instance-button" data-classification-index="${index}">Add Instance</button>
        <h4>Item Instances</h4>
        <div class="instances-container"></div>
      </div>`;
  container.insertAdjacentHTML("beforeend", newClassification);

  container
    .querySelector(
      `.classification-box[data-index="${index}"] .add-instance-button`
    )
    .addEventListener("click", addInstanceRow);

  container
    .querySelector(
      `.classification-box[data-index="${index}"] .delete-classification-button`
    )
    .addEventListener("click", deleteClassification);
}

function addInstanceRow(event) {
  const classificationIndex = event.target.dataset.classificationIndex;
  const container = document.querySelector(
    `.classification-box[data-index="${classificationIndex}"] .instances-container`
  );

  const instanceIndex = container.children.length;
  const newInstance = renderInstance({}, instanceIndex);
  container.insertAdjacentHTML("beforeend", newInstance);

  container
    .querySelector(
      `.instance-row[data-instance-index="${instanceIndex}"] .delete-instance-button`
    )
    .addEventListener("click", deleteInstanceRow);
}

function deleteClassification(event) {
  const classificationIndex = event.target.dataset.classificationIndex;
  const classificationBox = document.querySelector(
    `.classification-box[data-index="${classificationIndex}"]`
  );
  classificationBox.remove();
}

function deleteInstanceRow(event) {
  const instanceIndex = event.target.dataset.instanceIndex;
  const instanceRow = document.querySelector(
    `.instance-row[data-instance-index="${instanceIndex}"]`
  );
  instanceRow.remove();
}

async function submitItemDetail() {
  const categoryName = document.getElementById("categoryName").value;
  const classificationElements = document.querySelectorAll(".classification-box");

  const itemClassifications = Array.from(classificationElements).map((el) => {
    const classificationId = el.dataset.id || null; 
    const name = el.querySelector(".classification-name").value;

    const instances = Array.from(el.querySelectorAll(".instance-row")).map((row) => {
      return {
        id: row.dataset.id || null, 
        assetId: row.querySelector(".instance-asset-id").value,
      };
    });

    return { id: classificationId, name, itemInstances: instances };
  });

  const payload = {
    id: itemCategoryId,
    name: categoryName,
    itemClassifications,
  };

  try {
    const response = await fetch("http://localhost:5143/Item/Update", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });
    const result = await response.json();
    alert(result.message || "Update completed successfully!");
    window.location.href = `/Frontend/itemdetail.html?id=${itemCategoryId}`;
  } catch (error) {
    console.error("Error updating item detail:", error);
    alert("An error occurred while updating item details.");
  }
}

document
  .getElementById("addClassificationButton")
  .addEventListener("click", addClassification);
document
  .getElementById("submitButton")
  .addEventListener("click", submitItemDetail);

fetchItemDetail();
