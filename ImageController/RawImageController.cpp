#include "pch.h"

#include <memory>
#include <libraw/libraw.h>
#include "RawImageController.h"

namespace Kchary::ImageController::RawImageControl
{
    int RawImageController::GetImageData(const char* path, uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height)
    {
        const unique_ptr<LibRaw> rawProcessor = make_unique<LibRaw>();

        // Read raw image using libraw.
        if (rawProcessor->open_file(path) != LIBRAW_SUCCESS)
        {
            // Todo: Error sequence.
            return -1;
        }

        // Unpack the image.
        if (rawProcessor->unpack() != LIBRAW_SUCCESS)
        {
            // Todo: Error sequence.
            return -1;
        }

        if (rawProcessor->dcraw_process() != LIBRAW_SUCCESS)
        {
            // Todo: Error sequence.
            return -1;
        }

        const auto image = rawProcessor->dcraw_make_mem_image();
        if (!image || image->type != LIBRAW_IMAGE_BITMAP || image->colors != 3)
        {
            // Todo: Error sequence.
            return -1;
        }

        if (image->bits == 16 || image->bits == 8)
        {
            const auto dataSize = image->data_size;
            const auto rawData = (uint8_t*)image->data;

            *buffer = new uint8_t[dataSize];
            memcpy(*buffer, rawData, dataSize);

            *size = dataSize;
            *stride = image->width * image->colors * image->bits / 8;
            *width = image->width;
            *height = image->height;
        }
        else
        {
            //Todo: Error sequence.
            return -1;
        }

        // Free all internal data.
        LibRaw::dcraw_clear_mem(image);
        rawProcessor->recycle();

        return 0;
    }

    int RawImageController::GetThumbnailImageData(const char* path, int resizeLongSideLength, uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height)
    {
        const unique_ptr<LibRaw> rawProcessor = make_unique<LibRaw>();

        // Read raw image using libraw.
        if (rawProcessor->open_file(path) != LIBRAW_SUCCESS)
        {
            // Todo: Error sequence.
            return -1;
        }

        if (rawProcessor->unpack_thumb() != LIBRAW_SUCCESS)
        {
            // Todo: Error sequence.
            return -1;
        }

        // Get thumbnail struct data.
        libraw_processed_image_t* thumbnail = rawProcessor->dcraw_make_mem_thumb();
        if (!thumbnail || thumbnail->type != LibRaw_thumbnail_formats::LIBRAW_THUMBNAIL_JPEG)
        {
            // Todo: Error sequence.
            return -1;
        }

        // Read thumbnail jpeg image data using opencv.
        // (Raw image's thumbnail data is jpeg.)
        const auto thumbnailDataSize = thumbnail->data_size;
        const auto thumbnailData = (uint8_t*)thumbnail->data;

        const auto imreadMode = GetImreadMode(rawProcessor->imgdata.thumbnail, resizeLongSideLength);
        const vector<uint8_t> jpegData(thumbnailData, thumbnailData + thumbnailDataSize);
        cv::Mat img = cv::imdecode(jpegData, imreadMode);

        // Resize image data.
        int longSideLength = img.cols > img.rows ? img.cols : img.rows;
        double ratio = ((double)resizeLongSideLength / (double)longSideLength);
        cv::Mat resizeImg;
        cv::resize(img, resizeImg, cv::Size(), ratio, ratio, cv::INTER_AREA);

        const auto imgDataSize = resizeImg.total() * resizeImg.elemSize();
        *buffer = new uint8_t[imgDataSize];
        memcpy(*buffer, resizeImg.data, imgDataSize * sizeof(uint8_t));

        // Translate data to C#
        *size = (unsigned int)imgDataSize;
        *stride = (int)resizeImg.step;
        *width = resizeImg.cols;
        *height = resizeImg.rows;

        img.release();
        resizeImg.release();

        // Free all internal data.
        rawProcessor->dcraw_clear_mem(thumbnail);
        rawProcessor->recycle();

        return 0;
    }

    cv::ImreadModes RawImageController::GetImreadMode(libraw_thumbnail_t thumbnail, int resizeLongSideLength)
    {
        const ushort thumbLongSideLength = thumbnail.twidth > thumbnail.theight ? thumbnail.twidth : thumbnail.theight;

        auto imreadMode = cv::ImreadModes::IMREAD_COLOR;
        if (resizeLongSideLength <= thumbLongSideLength / 8)
        {
            imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_8;
        }
        else if (thumbLongSideLength / 8 < resizeLongSideLength && resizeLongSideLength <= thumbLongSideLength / 4)
        {
            imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_4;
        }
        else if (thumbLongSideLength / 4 < resizeLongSideLength && resizeLongSideLength <= thumbLongSideLength / 2)
        {
            imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_2;
        }
        else
        {
            imreadMode = cv::ImreadModes::IMREAD_COLOR;
        }

        return imreadMode;
    }
}