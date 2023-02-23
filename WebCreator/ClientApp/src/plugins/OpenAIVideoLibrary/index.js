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

const openAIVideoLibrary = {
  name: "openAIVideoLibrary",
  display: "dialog",
  title: "OpenAI Videos",
  listClass: 'se-image-list',
  innerHTML: `
  <svg viewBox="30 30 150 150" xmlns="http://www.w3.org/2000/svg"> <g>  <title>Layer 1</title>  <rect stroke="#000" id="svg_2" height="58" width="179" y="120.99999" x="0.99999" stroke-width="0" fill="#ffffff"/>  <text transform="matrix(2.95354 0 0 1.58398 11.6123 -84.457)" stroke="#000" font-weight="bold" xml:space="preserve" text-anchor="start" font-family="Noto Sans JP" font-size="55" id="svg_3" y="166.54228" x="-4.00001" stroke-width="0" fill="#000000">AI</text>  <g stroke="null">   <g stroke="null">    <path stroke="null" transform="translate(-4.41 -4.35)" d="m153.34235,47.84666l0,64.9873l-29.63541,-11.80903l0,9.02601l-88.7558,0l0,-59.19561l88.83102,0l0,9.25166l15.04336,-6.09256l15.04336,-6.16778l-0.52652,0zm-41.44445,50.69611l0,0l0,-33.54668l0,0l0,-2.33172l-64.9873,0l0,35.87841l65.21295,0l-0.22565,0zm29.63541,-2.78302l0,-30.08671l-17.82638,7.52168l0,15.04336l8.87558,3.61041l8.9508,3.61041l0,0.30087zm-80.03066,-50.92176a20.83505,20.83505 0 0 1 -20.68462,-20.68462a20.75983,20.75983 0 0 1 41.44445,0a20.91027,20.91027 0 0 1 -20.75983,20.68462zm0,-29.5602a9.10123,9.10123 0 0 0 -6.24299,2.63259a8.64993,8.64993 0 0 0 -2.55737,6.31821a8.19863,8.19863 0 0 0 2.55737,6.61908a8.64993,8.64993 0 0 0 6.69429,2.10607a8.4995,8.4995 0 0 0 8.34906,-8.72515a8.72515,8.72515 0 0 0 -8.80036,-8.9508zm44.3779,29.5602a17.60073,17.60073 0 0 1 -12.5612,-5.11474a17.29986,17.29986 0 0 1 -5.11474,-12.5612a17.67594,17.67594 0 0 1 30.08671,-12.5612a17.82638,17.82638 0 0 1 0,25.12241a17.52551,17.52551 0 0 1 -12.63642,5.11474l0.22565,0zm0,-23.61807a5.64126,5.64126 0 1 0 4.13692,1.65477a5.49083,5.49083 0 0 0 -4.13692,-1.65477z"/>   </g>  </g> </g></svg>
  `,
  buttonClass: "",
  add: function(core) {
    core.addModule([dialog]);

    const context = core.context;
    context.openAIVideoLibrary = {};

    let media_dialog = this.setDialog.call(core);
    media_dialog.addEventListener('click', this.onClick_linkController.bind(core));
    context.openAIVideoLibrary.modal = media_dialog;

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
            ${lang.dialogBox.videoBox.title}
          </span>
          <div class="se-dialog-form">
              <div class="se-dialog-form-files" style="padding-left: 20px;padding-right: 20px;">
                <input id="openAIKeyword" class="se-input-form se-input-url openAIKeyword" type="text" placeholder="">
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
    this.plugins.openAIVideoLibrary.loadImages.call(this, this.context);
  },
  init: function() {},
  loadImages: function(core) {
    const obj = this;
    const modal = core.openAIVideoLibrary.modal;
    const loader = modal.querySelector(".loader");
    const listImages = modal.querySelector(".listImages");
    const lazyImage = lazyImageObserver();

    loader.classList.remove("hide");
    listImages.classList.add("hide");

    this.context.openAIVideoLibrary._xmlHttp = this.util.getXMLHttpRequest();

    this.context.openAIVideoLibrary._xmlHttp.onreadystatechange = function() {

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
            obj.plugins.openAIVideoLibrary.addImage.call(obj, event);
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

    this.context.openAIVideoLibrary._xmlHttp.open(
      "get",
      this.context.option.openAIVideoLoadURL,
      true
    );

    if (this.context.option.requestHeaders) {
      const requestHeaders = this.context.option.requestHeaders;

      Object.entries(requestHeaders).forEach(([key, value]) => {
        this.context.openAIVideoLibrary._xmlHttp.setRequestHeader(key, value);
      });
    }

    this.context.openAIVideoLibrary._xmlHttp.send();
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
      "openAIVideoLibrary",
      "openAIVideoLibrary" === this.currentControllerName
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
        const modal = this.context.openAIVideoLibrary.modal;
        const openAIKeyword = modal.querySelector(".openAIKeyword");
        //console.log(openAIKeyword.value.replaceAll(' ', "+").replaceAll('?', ''));
        const keyword = openAIKeyword.value.replaceAll(' ', "+").replaceAll('?', '');
        this.context.option.openAIVideoLoadURL = `${process.env.REACT_APP_SERVER_URL}openAI/video/10?prompt=${keyword}`;
        this.plugins.openAIVideoLibrary.loadImages.call(this, this.context);
        break;
      }
    }
  },
  update: function(event) {
    console.log('update');
  },
};

export default openAIVideoLibrary;
