import dialog from "suneditor/src/plugins/modules/dialog";

import "./styles.css";

export const lazyImageObserver = () => {
  if ("IntersectionObserver" in window) {
    return new IntersectionObserver(function(entries, observer) {
      const _this = this;
      entries.forEach(function(entry) {
        if (entry.isIntersecting) {
           let lazyImage = entry.target;
        //   lazyImage.src = lazyImage.dataset.src;
        //   lazyImage.srcset = lazyImage.dataset.srcset;
           lazyImage.parentNode.classList.remove("loading");
          _this.unobserve(lazyImage);
        }
      });
    });
  }
};

const pixabayImageGallery = {
  name: "pixabayImageGallery",
  display: "dialog",
  title: "Images",
  listClass: 'se-image-list',
  innerHTML: `
  <svg xmlns="http://www.w3.org/2000/svg" viewBox="30 30 150 150"><g><path d="M152.775,120.548V51.651c0-12.271-9.984-22.254-22.254-22.254H43.727c-12.271,0-22.254,9.983-22.254,22.254v68.896c0,12.27,9.983,22.254,22.254,22.254h86.795C142.791,142.802,152.775,132.817,152.775,120.548z M36.394,51.651c0-4.042,3.291-7.333,7.333-7.333h86.795c4.042,0,7.332,3.291,7.332,7.333v23.917l-14.938-17.767c-1.41-1.678-3.487-2.649-5.68-2.658h-0.029c-2.184,0-4.255,0.954-5.674,2.613L76.709,98.519l-9.096-9.398c-1.427-1.474-3.392-2.291-5.448-2.273c-2.052,0.025-4.004,0.893-5.396,2.4L36.394,111.32V51.651z M41.684,127.585l20.697-22.416l9.312,9.622c1.461,1.511,3.489,2.334,5.592,2.27c2.101-0.066,4.075-1.013,5.44-2.612l34.436-40.308l20.693,24.613v21.794c0,4.042-3.29,7.332-7.332,7.332H43.727C43.018,127.88,42.334,127.775,41.684,127.585z M182.616,152.5V75.657c0-4.12-3.34-7.46-7.461-7.46c-4.119,0-7.46,3.34-7.46,7.46V152.5c0,4.112-3.347,7.46-7.461,7.46h-94c-4.119,0-7.46,3.339-7.46,7.459c0,4.123,3.341,7.462,7.46,7.462h94C172.576,174.881,182.616,164.841,182.616,152.5z"/></g></svg>
  `,
  buttonClass: "",
  add: function(core) {
    core.addModule([dialog]);

    const context = core.context;
    context.pixabayImageGallery = {};

    let media_dialog = this.setDialog.call(core);
    media_dialog.addEventListener('click', this.onClick_linkController.bind(core));
    context.pixabayImageGallery.modal = media_dialog;

    context.dialog.modal.appendChild(media_dialog);

    media_dialog = null;
  },
  setDialog: function() {
    const lang = this.lang;
    const dialog = this.util.createElement("DIV");
    this.util.addClass(dialog, "se-dialog-content");

    dialog.style.display = "none";
    dialog.innerHTML = `
        <div class="se-dialog-header">
          <button
            type="button"
            data-command="close"
            class="se-btn se-dialog-close"
            aria-label="${lang.dialogBox.close}"
            title="${lang.dialogBox.close}"
          >
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 15.74 15.74">
              <g><path d="M14.15,11.63l5.61,5.61a1.29,1.29,0,0,1,.38.93,1.27,1.27,0,0,1-.4.93,1.25,1.25,0,0,1-.92.4,1.31,1.31,0,0,1-.94-.4l-5.61-5.61L6.67,19.1a1.31,1.31,0,0,1-.94.4,1.24,1.24,0,0,1-.92-.4,1.27,1.27,0,0,1-.4-.93,1.33,1.33,0,0,1,.38-.93l5.61-5.63L4.79,6a1.26,1.26,0,0,1-.38-.93,1.22,1.22,0,0,1,.4-.92,1.28,1.28,0,0,1,.92-.39,1.38,1.38,0,0,1,.94.38l5.61,5.61,5.61-5.61a1.33,1.33,0,0,1,.94-.38,1.26,1.26,0,0,1,.92.39,1.24,1.24,0,0,1,.4.92,1.29,1.29,0,0,1-.39.93L17,8.81l-2.8,2.82Z" transform="translate(-4.41 -3.76)"></path></g>
            </svg>
          </button>
          <span class="se-modal-title">
            ${lang.dialogBox.imageBox.title}
          </span>
          <div class="se-dialog-form">
              <div class="se-dialog-form-files" style="padding-left: 20px;padding-right: 20px;">
                <input id="pixabayKeyword" class="se-input-form se-input-url pixabayKeyword" type="text" placeholder="">
                  <button type="button" data-command="update" class="se-btn se-dialog-files-edge-button _se_bookmark_button">
                      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 15.73 15.75"><g><path d="M19.86,19.21a1,1,0,0,0,.28-.68,1,1,0,0,0-.28-.7L13,10.93a1,1,0,0,0-.7-.28,1,1,0,0,0-.68,1.65l6.9,6.9a1,1,0,0,0,.69.29.93.93,0,0,0,.69-.28ZM9.19,8.55a3,3,0,0,0,1.68,0,14.12,14.12,0,0,0,1.41-.32A11.26,11.26,0,0,0,10.8,7.06c-.56-.36-.86-.56-.91-.58S10,5.91,10,5.11s0-1.26-.15-1.37a4.35,4.35,0,0,0-1.19.71c-.53.4-.81.62-.87.68a9,9,0,0,0-2-.6,6.84,6.84,0,0,0-.76-.09s0,.27.08.77a8.6,8.6,0,0,0,.61,2q-.09.09-.69.87a3.59,3.59,0,0,0-.68,1.17c.12.17.57.23,1.36.15S7,9.26,7.15,9.23s.21.36.57.91a10.49,10.49,0,0,0,1.14,1.48c0-.1.14-.57.31-1.4a3,3,0,0,0,0-1.67Z" transform="translate(-4.41 -3.74)"/></g></svg>
                  </button>
                  <div class="se-select-list" style="display: none;"></div>
              </div>
          </div>
        </div>
        <div class="se-dialog-body se-dialog-fixed-body">
          <div class="loader">
            <div class="loader-wrapper">
              <svg class="loader-svg loader-animation" viewBox="22 22 44 44">
                <circle class="loader-circle" cx="44" cy="44" r="20.2" fill="none" stroke-width="3.6"></circle>
              </svg>
            </div>
          </div>
          <div class="listImages"></div>
        </div>
        <div class="se-dialog-footer">
          <button type="button" data-command="close" class="se-btn-primary">
            <span>${lang.dialogBox.close}</span>
          </button>
        </div>
      `;

    return dialog;
  },
  close: function(event) {
    event.preventDefault();
    event.stopPropagation();

    this.plugins.dialog.close.call(this);
  },
  on: function() {
    this.plugins.pixabayImageGallery.loadImages.call(this, this.context);
  },
  init: function() {},
  loadImages: function(core) {
    const obj = this;
    const modal = core.pixabayImageGallery.modal;
    const loader = modal.querySelector(".loader");
    const listImages = modal.querySelector(".listImages");
    const lazyImage = lazyImageObserver();

    loader.classList.remove("hide");
    listImages.classList.add("hide");

    this.context.pixabayImageGallery._xmlHttp = this.util.getXMLHttpRequest();

    this.context.pixabayImageGallery._xmlHttp.onreadystatechange = function() {

      if (this.readyState === 4 && this.status === 200) {
        let json = null
        try {
          json = JSON.parse(this.responseText);
        }
        catch(err) {
          console.log(err.message)
          return
        }

        listImages.innerHTML = "";

        json.hits.forEach(img => {
          const { previewURL, largeImageURL, tags } = img;
          const url = previewURL;
          const caption = tags;

          const anchor = document.createElement("DIV");
          obj.util.addClass(anchor, "wrapperImage loading");

          const image = document.createElement("img");
          image.src = url;
          image.orgsrc = largeImageURL;

          const imgCaption = document.createElement("DIV");
          obj.util.addClass(imgCaption, "imgCaption");
          imgCaption.innerHTML = caption;

          image.addEventListener("click", function(event) {
            obj.plugins.pixabayImageGallery.addImage.call(obj, event);
          });

          anchor.appendChild(image);
          anchor.appendChild(imgCaption);
          listImages.appendChild(anchor);

          /** Apply lazy loading images */
          Array.from(document.querySelectorAll(".wrapperImage > img")).forEach(
            image => lazyImage.observe(image)
          );
        });

        loader.classList.add("hide");
        listImages.classList.remove("hide");
      }
    };

    this.context.pixabayImageGallery._xmlHttp.open(
      "get",
      this.context.option.imageGalleryLoadURL,
      true
    );

    if (this.context.option.requestHeaders) {
      const requestHeaders = this.context.option.requestHeaders;

      Object.entries(requestHeaders).forEach(([key, value]) => {
        this.context.pixabayImageGallery._xmlHttp.setRequestHeader(key, value);
      });
    }

    this.context.pixabayImageGallery._xmlHttp.send();
  },
  addImage: function(event) {
    const imgsrc = event.srcElement.orgsrc;

    /**  https://github.com/JiHong88/SunEditor/blob/d155b1d7d4437002e52b4502a621e25699791f97/src/plugins/dialog/image.js#L537 */
    this.plugins.image.create_image.call(
      this,
      imgsrc,
      "",
      false,
      0,
      0,
      "none",
      null
    );

    this.plugins.dialog.close.call(this);
  },
  open: function() {
    this.plugins.dialog.open.call(
      this,
      "pixabayImageGallery",
      "pixabayImageGallery" === this.currentControllerName
    );
  },
  onClick_linkController: function (e) {
    e.stopPropagation();

    const command = e.target.getAttribute('data-command') || e.target.parentNode.getAttribute('data-command');
    if (!command) return;

    e.preventDefault();
    //console.log(command);
    switch(command){
      case "close":
      {
        this.plugins.dialog.close.call(this);
        break;
      }
      case "update":
      {
        const modal = this.context.pixabayImageGallery.modal;
        const pixabayKeyword = modal.querySelector(".pixabayKeyword");
        //console.log(pixabayKeyword.value.replaceAll(' ', "+").replaceAll('?', ''));
        const keyword = pixabayKeyword.value.replaceAll(' ', "+").replaceAll('?', '');
        this.context.option.imageGalleryLoadURL = 'https://pixabay.com/api/?key=14748885-e58fd7b3b1c4bf5ae18c651f6&q=' + keyword + '&image_type=photo&min_width=480&min_height=600&per_page=100&page=1'
        this.plugins.pixabayImageGallery.loadImages.call(this, this.context);
        break;
      }
    }
  },
  update: function(event) {
    console.log('update');
  },
};

export default pixabayImageGallery;
