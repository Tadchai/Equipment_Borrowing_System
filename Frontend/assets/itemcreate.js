let classifications = [];

function addClassification() {
  const classificationId = classifications.length;
  classifications.push({ id: classificationId, instances: [] });

  const classificationDiv = document.createElement("div");
  classificationDiv.id = `classification-${classificationId}`;
  classificationDiv.innerHTML = `
    <label>Item Classification Name:</label>
    <input type="text" id="classificationName-${classificationId}" />
    <button type="button" onclick="addInstance(${classificationId})">Add Instance</button>
    <div id="instances-${classificationId}"></div>
    <button type="button" onclick="removeClassification(${classificationId})">Delete</button>
  `;
  document.getElementById("classifications").appendChild(classificationDiv);
}

function addInstance(classificationId) {
  const instanceId = classifications[classificationId].instances.length;
  classifications[classificationId].instances.push({ id: instanceId });

  const instanceDiv = document.createElement("div");
  instanceDiv.id = `instance-${classificationId}-${instanceId}`;
  instanceDiv.innerHTML = `
    <label>Asset ID:</label>
    <input type="text" id="assetId-${classificationId}-${instanceId}" />
    <button type="button" onclick="removeInstance(${classificationId}, ${instanceId})">Delete</button>
  `;
  document.getElementById(`instances-${classificationId}`).appendChild(instanceDiv);
}

function removeClassification(classificationId) {
  classifications = classifications.filter((c) => c.id !== classificationId);
  document.getElementById(`classification-${classificationId}`).remove();
}

function removeInstance(classificationId, instanceId) {
  classifications[classificationId].instances = classifications[classificationId].instances.filter(
    (i) => i.id !== instanceId
  );
  document.getElementById(`instance-${classificationId}-${instanceId}`).remove();
}

document.getElementById("itemForm").addEventListener("submit", async (e) => {
    e.preventDefault();
  
    const categoryName = document.getElementById("categoryName").value;
    const itemClassifications = classifications.map((c) => {
      const classificationName = document.getElementById(`classificationName-${c.id}`).value;
      const itemInstances = c.instances.map((i) => {
        const assetId = document.getElementById(`assetId-${c.id}-${i.id}`).value;
        return { assetId };
      });
      return { name: classificationName, itemInstances };
    });
  
    const payload = {
      name: categoryName,
      itemClassifications,
    };
  
    try {
      const response = await fetch("http://localhost:5143/Item/Create", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });
  
      if (!response.ok) {
        throw new Error(`HTTP error! Status: ${response.status}`);
      }
  
      const result = await response.json();
  
      if (result.statusCode === 201) { 
        const idCategory = result.id; 
        alert(result.message);
        window.location.href = `/Frontend/itemdetail.html?id=${idCategory}`;
      } else {
        alert(result.message || "Something went wrong");
      }
    } catch (error) {
      console.error("Error:", error);
      alert("An error occurred while creating the item.");
    }
  });
  
  
  