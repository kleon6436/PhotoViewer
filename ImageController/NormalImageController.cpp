#include "pch.h"

#include <memory>
#include "NormalImageController.h"

namespace Kchary::ImageController::NormalImageControl
{
    int NormalImageController::GetImageData(const char* path, uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height)
    {
        // Read image data to mat.
        cv::Mat img = cv::imread(path, cv::ImreadModes::IMREAD_COLOR);
        
        const auto dataSize = img.total() * img.elemSize();
        *buffer = new uint8_t[dataSize];
        memcpy(*buffer, img.data, dataSize * sizeof(uint8_t));

        // Translate data to C#
        *size = (unsigned int)dataSize;
        *stride = (int)img.step;
        *width = img.cols;
        *height = img.rows;

        img.release();

        return 0;
    }

    int NormalImageController::GetThumbnailImageData(const char* path, int resizeLongSideLength, uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height)
    {
        // Read image data to mat.
        const auto imreadMode = GetImreadMode(resizeLongSideLength);
        cv::Mat img = cv::imread(path, imreadMode);

        // Resize image data.
        const int longSideLength = img.cols > img.rows ? img.cols : img.rows;
        const double ratio = ((double)resizeLongSideLength / (double)longSideLength);
        cv::Mat resizeImg;
        cv::resize(img, resizeImg, cv::Size(), ratio, ratio, cv::INTER_AREA);

        const auto dataSize = resizeImg.total() * resizeImg.elemSize();
        *buffer = new uint8_t[dataSize];
        memcpy(*buffer, resizeImg.data, dataSize * sizeof(uint8_t));

        // Translate data to C#
        *size = (unsigned int)dataSize;
        *stride = (int)resizeImg.step;
        *width = resizeImg.cols;
        *height = resizeImg.rows;

        img.release();
        resizeImg.release();

        return 0;
    }

    cv::ImreadModes NormalImageController::GetImreadMode(int resizeLongSideLength)
    {
        auto imreadMode = cv::ImreadModes::IMREAD_COLOR;
        if (resizeLongSideLength <= 800)
        {
            imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_8;
        }
        else if (800 < resizeLongSideLength && resizeLongSideLength <= 1600)
        {
            imreadMode = cv::ImreadModes::IMREAD_REDUCED_COLOR_4;
        }
        else if (1600 < resizeLongSideLength && resizeLongSideLength <= 3200)
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