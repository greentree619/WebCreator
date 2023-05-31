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

const openAIImageGallery = {
  name: "openAIImageGallery",
  display: "dialog",
  title: "OpenAI Images",
  listClass: 'se-image-list',
  innerHTML: `
  <svg viewBox="30 30 150 150" xmlns="http://www.w3.org/2000/svg"> <g>  <title>Layer 1</title>  <g stroke="null">   <g stroke="null">    <path stroke="null" d="m131.286,69.39806l0,-51.15816c0,-9.1116 -8.12528,-16.52428 -18.11097,-16.52428l-70.63556,0c-9.98651,0 -18.11097,7.41269 -18.11097,16.52428l0,51.15742c0,9.11086 8.12446,16.52428 18.11097,16.52428l70.63638,0c9.98488,0.00074 18.11016,-7.41343 18.11016,-16.52354zm-94.71435,-51.15816c0,-3.00131 2.67831,-5.44498 5.96782,-5.44498l70.63638,0c3.2895,0 5.967,2.44367 5.967,5.44498l0,17.75911l-12.15699,-13.19255c-1.1475,-1.24597 -2.83783,-1.96696 -4.62255,-1.97365l-0.0236,0c-1.7774,0 -3.46285,0.70837 -4.61767,1.94023l-28.34082,30.26779l-7.4026,-6.97831c-1.16134,-1.09449 -2.76051,-1.70114 -4.43375,-1.68777c-1.66998,0.01856 -3.25858,0.66308 -4.39143,1.78207l-16.58179,16.38914l0,-44.30608zm4.30516,56.38335l16.84384,-16.64457l7.57838,7.14463c1.18901,1.12196 2.83945,1.73307 4.55094,1.68555c1.70986,-0.04901 3.31636,-0.75218 4.42724,-1.93949l28.02505,-29.92994l16.84058,18.27592l0,16.18272c0,3.00131 -2.6775,5.44424 -5.967,5.44424l-70.63638,0c-0.57701,0 -1.13367,-0.07797 -1.66265,-0.21905zm114.69469,18.50016l0,-57.05831c0,-3.05923 -2.71819,-5.53928 -6.07199,-5.53928c-3.35217,0 -6.07117,2.48005 -6.07117,5.53928l0,57.05831c0,3.05329 -2.72389,5.53928 -6.07199,5.53928l-76.50002,0c-3.35217,0 -6.07117,2.47931 -6.07117,5.53854c0,3.06146 2.71901,5.54077 6.07117,5.54077l76.50002,0c10.04429,0 18.21514,-7.45501 18.21514,-16.61859z"/>   </g>  </g>  <rect stroke="#000" id="svg_2" height="58" width="179" y="120.99999" x="0.99999" stroke-width="0" fill="#ffffff"/>  <text transform="matrix(2.95354 0 0 1.58398 11.6123 -84.457)" stroke="#000" font-weight="bold" xml:space="preserve" text-anchor="start" font-family="Noto Sans JP" font-size="55" id="svg_3" y="166.54228" x="-4.00001" stroke-width="0" fill="#000000">AI</text> </g></svg>
  `,
  buttonClass: "",
  add: function(core) {
    core.addModule([dialog]);

    const context = core.context;
    context.openAIImageGallery = {};

    let media_dialog = this.setDialog.call(core);
    media_dialog.addEventListener('click', this.onClick_linkController.bind(core));
    context.openAIImageGallery.modal = media_dialog;

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
    this.plugins.openAIImageGallery.loadImages.call(this, this.context);
  },
  init: function() {},
  loadImages: function(core) {
    const obj = this;
    const modal = core.openAIImageGallery.modal;
    const loader = modal.querySelector(".loader");
    const listImages = modal.querySelector(".listImages");
    const lazyImage = lazyImageObserver();

    loader.classList.remove("hide");
    listImages.classList.add("hide");

    this.context.openAIImageGallery._xmlHttp = this.util.getXMLHttpRequest();

    this.context.openAIImageGallery._xmlHttp.onreadystatechange = function() {

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

        json.data.forEach(img => {
          console.log(img)
          const { thumb, url } = img;

          const anchor = document.createElement("DIV");
          obj.util.addClass(anchor, "wrapperImage loading");

          const image = document.createElement("img");
          image.src = thumb;
          image.orgsrc = url;

          const imgCaption = document.createElement("DIV");
          obj.util.addClass(imgCaption, "imgCaption");
          imgCaption.innerHTML = "";

          image.addEventListener("click", function(event) {
            obj.plugins.openAIImageGallery.addImage.call(obj, event);
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

    this.context.openAIImageGallery._xmlHttp.open(
      "get",
      this.context.option.openAIImageLoadURL,
      true
    );

    if (this.context.option.requestHeaders) {
      const requestHeaders = this.context.option.requestHeaders;

      Object.entries(requestHeaders).forEach(([key, value]) => {
        this.context.openAIImageGallery._xmlHttp.setRequestHeader(key, value);
      });
    }

    this.context.openAIImageGallery._xmlHttp.send();
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
    console.log(this.currentControllerName)
    this.plugins.dialog.open.call(
      this,
      "openAIImageGallery",
      "openAIImageGallery" === this.currentControllerName
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
        const modal = this.context.openAIImageGallery.modal;
        const openAIKeyword = modal.querySelector(".openAIKeyword");
        //console.log(openAIKeyword.value.replaceAll(' ', "+").replaceAll('?', ''));
        const keyword = openAIKeyword.value.replaceAll(' ', "+").replaceAll('?', '');
        this.context.option.openAIImageLoadURL = `${process.env.REACT_APP_SERVER_URL}openAI/image/10?prompt=${keyword}`
        this.plugins.openAIImageGallery.loadImages.call(this, this.context);
        break;
      }
    }
  },
  update: function(event) {
    console.log('update');
  },
};

export default openAIImageGallery;
